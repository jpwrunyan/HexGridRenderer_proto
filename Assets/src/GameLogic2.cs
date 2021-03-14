using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// This is a temporary initializer for the team display stuff. Could be attached to the UI Canvas instead of an empty object.
/// 
/// Terminology: pass/end turn may be "fold".
/// Pass/end turn means you "preserve your initiative" for the next round (ie go sooner).
/// Actions will eventually have "initiative cost". or deficit?
/// Those with lowest accumulated initiative cost go first.
/// Opposite of initiative is inertia?
/// 
/// </summary>
[RequireComponent(typeof(AnimationManager))]
public class GameLogic2 : MonoBehaviour, InputSource {
	//The following are UI state. NOT game or battle state.
	private const int DISABLED = 0; //when in disabled state, input should be automatically determined.
	private const int SELECT_CARD = 1;
	private const int SELECT_MOVE = 2;
	private const int SELECT_TARGET = 3;

	public TurnOrderDisplay turnOrderDisplay;
	public CardRenderer selectedCardRenderer;
	public CardListDisplay cardListDisplay;
	public HexGridRenderer hexGridRenderer;
	public HexPathRenderer hexPathRenderer;
	public GameObject indicator;

	public Text infoText;
	public Text deckText;
	public Text discardText;
	public Button passButton;

	
	public BattleState battleState;
	
	//Input state determines what controls and display are enabled.
	private int uiInputState = 1;

	private Vector2Int selectedHexXY;
	private Vector2Int[] validTargets;

	private GameInput controls;

	private Logger logger = Logger.getInstance();

	private AnimationManager animationManager;

	private SimpleAutoInput simpleAutoInput = new SimpleAutoInput();

	private void Awake() {

		GameState gameState = GameState.getInstance();
		
		Debug.Log("id should be sound: " + gameState.id);
		init();

		controls = new GameInput();

		controls.UI.Navigate.performed += Navigate_performed;

		controls.UI.Click.performed += Click_performed;
		controls.UI.RightClick.performed += RightClick_performed;
		controls.UI.Submit.performed += Submit_performed;
		controls.UI.Cancel.performed += Cancel_performed;
		controls.UI.Point.performed += Point_performed;
		controls.UI.MiddleClick.performed += MiddleClick_performed;
		controls.UI.ScrollWheel.performed += ScrollWheel_performed;

		controls.Player.Disable();
		controls.UI.Enable();
	}

	private void init() {
		GameState gameState = GameState.getInstance();
		battleState = gameState.battleState;

		hexGridRenderer.hexGrid = battleState.hexGrid;
		
		hexGridRenderer.battlefieldEntities = battleState.battlefieldEntities;

		hexGridRenderer.imageLibrary = gameState.imageLibrary;

		hexGridRenderer.updateDisplay();

		animationManager = GetComponent<AnimationManager>();
		
		//Center camera on arena.
		Vector3 centerHexPos = HexGridRenderer.getXYZPos(4, 5);
		Camera.main.transform.position = centerHexPos;

		battleState.setInputSource(this);
	}

	/// <summary>
	/// For now this mixes both view and control logic.
	/// It is called by battle state once this input source is assigned to it.
	/// </summary>
	public void processNextAction(BattleState battleState) {
		//Debug.Log("processNextAction: " + battleState.getCurrentAction().type.ToString());
		GameState gameState = GameState.getInstance();
		turnOrderDisplay.updateDisplay(battleState, gameState.imageLibrary);

		if (battleState.isCombatOver()) {
			//Somehow, all combatants are no longer active (killed each other?)
			//This battle cannot continue.
			infoText.text = "Round " + battleState.round + " Turn " + battleState.turn + " - Battle Over";
			return;
		}

		//Debug.Log("processNextAction: " + battleState.getCurrentCombatant().name + " - " + battleState.getActionState().ToString());
		if (animationManager.isPlaying && battleState.pendingActions.Count > battleState.actionPhase) {
			indicator.SetActive(false);
			//Disable and delay next action until animation(s) are complete.
			uiInputState = DISABLED;
			animationManager.onComplete = () => processNextAction(battleState);
			return;
		} else {
			indicator.SetActive(true);
			animationManager.onComplete = () => updateIndicatorPosition();
		}
		
		Debug.Log("process next action: " + battleState.getActionState() + " prev ui state: " + getInputName());

		//Update display information
		deckText.text = "Deck: " + battleState.getCurrentCombatant().deck.getDrawsRemaining();
		discardText.text = "Discard: " + battleState.getCurrentCombatant().deck.getDiscardCount();

		infoText.text = "Round " + battleState.round + " Turn " + battleState.turn + " - " + battleState.getCurrentCombatant().name + " " + getInputStateName() + "\n";
		infoText.text += "[";
		
		for (int i = 0; i < battleState.getCombatantsInTurnOrder().Count; i++) {
			BattlefieldEntity combatant = battleState.getCombatantsInTurnOrder()[i];
			bool isCurrentCombatant = combatant == battleState.getCurrentCombatant();
			infoText.text += (isCurrentCombatant ? ">" : " ") + combatant.name + (isCurrentCombatant ? "<" : " ");
		}
		
		infoText.text += "]\n";

		if (selectedCardRenderer.visible) {
			infoText.text += "Played: " + selectedCardRenderer.getTitle().Replace("\n", " ");
		}

		updateIndicatorPosition();
		//Accept input
		if (!battleState.getCurrentCombatant().isAI) {
			switch (battleState.getActionState()) {
				case CombatActionType.SELECT_ACTION: {
					//Show the card selection options
					selectedCardRenderer.gameObject.SetActive(false);
					cardListDisplay.gameObject.SetActive(true);
					cardListDisplay.clearSelection();
				
					List<Card> cards = battleState.getCurrentCombatant().deck.getHand();
					cardListDisplay.setCards(cards);
					uiInputState = SELECT_CARD;
					break;
				}
				case CombatActionType.MOVE: {
					hexPathRenderer.gameObject.SetActive(true);
					hexPathRenderer.pathPos = new List<Vector2Int>();
					hexPathRenderer.updateOnDemand();
					uiInputState = SELECT_MOVE;
					break;
				}
				case CombatActionType.MELEE_ATTACK:
				case CombatActionType.RANGE_ATTACK: {
					validTargets = HexGrid.getVisibleHexes(
						battleState.getCurrentCombatant().pos,
						battleState.getBlockedHexes(1).ToArray(),
						battleState.getCurrentAction().minRange,
						battleState.getCurrentAction().maxRange
					);
					hexGridRenderer.setHilight2(
						validTargets
					);
					hexGridRenderer.reDrawColor();
					uiInputState = SELECT_TARGET;
					break;
				}
				default: {
					uiInputState = DISABLED;
					Debug.Log("ui state is unknown");
					break;
				}
			}
		} else {
			uiInputState = DISABLED;
			if (battleState.getActionState().Equals(CombatActionType.SELECT_ACTION)) {
				//Debug.Log("auto select card");
				cardListDisplay.selectedIndex = simpleAutoInput.getSelectCardInput(battleState).value.x;
				commitCardSelection();
				//simpleAutoInput.processNextAction(battleState);
			} else if (battleState.getActionState().Equals(CombatActionType.MOVE)) {
				//Debug.Log("auto select move");
				//simpleAutoInput.processNextAction(battleState);
				selectedHexXY = simpleAutoInput.getMoveInput(battleState).value;
				updateHexPathRenderer();
				commitMoveSelection();
			} else if (
				battleState.getActionState().Equals(CombatActionType.MELEE_ATTACK) || 
				battleState.getActionState().Equals(CombatActionType.RANGE_ATTACK)
			) {
				//Debug.Log("auto select target");
				selectedHexXY = simpleAutoInput.getSelectTargetInput(battleState).value;
				Debug.Log("select target at: " + selectedHexXY.x + ", " + selectedHexXY.y);
				commitTargetSelection();
			}
		}
	}

	private int indicatorQuitCount = 0;

	private void updateIndicatorPosition() {
		//Update inidicator
		if (battleState.getCombatantsInTurnOrder().Count == 0) {
			Debug.Log("All combatants have been disabled. Disabling turn indicator. " + indicatorQuitCount++);
			indicator.SetActive(false);
			return;
		}
		Vector3 pivot = hexGridRenderer.getEntityRendererByName(battleState.getCurrentCombatant().name).transform.position;
		indicator.transform.rotation = Camera.main.transform.rotation;
		Vector3 point = pivot + (Camera.main.transform.up * 50);
		indicator.gameObject.transform.position = point;
	}

	public void Update() {
		//Might be worthwhile to move this to the indicator script so that when it's not active it doesn't get called.
		indicator.transform.Rotate(Vector3.up * (50 * Time.deltaTime));
	}

	private void commitCardSelection() {
		Card selectedCard = battleState.getCurrentCombatant().deck.getHand()[cardListDisplay.selectedIndex];
		//Might move these commands to an updateDisplay method
		cardListDisplay.hideCard(cardListDisplay.selectedIndex);
		selectedCardRenderer.setCard(selectedCard);
		selectedCardRenderer.gameObject.SetActive(true);
		cardListDisplay.gameObject.SetActive(false);
		BattleInput input = new BattleInput();
		//input.combatantId = selectedCard.combatantId;
		input.value = new Vector2Int(cardListDisplay.selectedIndex, 0);
		battleState.processActionInput(input);
	}

	private void commitMoveSelection() {
		//Debug.Log("commitMoveSelection " + battleState.getCurrentCombatant().name + " move to: " + selectedHexXY);
		hexGridRenderer.clearHilight();
		hexGridRenderer.reDrawColor();
		
		EntityRenderer target = hexGridRenderer.getEntityRendererByName(battleState.getCurrentCombatant().name);

		//Num of hex lengths (non-manhattan)


		//This part needs to be fixed:
		//For auto-movement, we don't use the hexPathRenderer... arg. So complicated. Movement should fall under combat effects.
		if (hexPathRenderer.pathPos.Count == 0) {
			Debug.Log("TERRIBE PROBLEM!");
		} else {
			float d = Vector2.Distance(battleState.getCurrentCombatant().pos, hexPathRenderer.pathPos[0]);

			if (d > 0) {
				d += 1 / d;
				d /= 2;

				Vector3 from = HexGridRenderer.getXYZPos(battleState.getCurrentCombatant().pos);
				Vector3 to = HexGridRenderer.getXYZPos(hexPathRenderer.pathPos[0]);

				animationManager.queueAnimation(new AnimationManager.MoveAnimation(target.gameObject, from, to, d));
			}
		}
		hexPathRenderer.gameObject.SetActive(false);
		BattleInput input = new BattleInput();
		//input.combatantId = battleState.getCurrentCombatantId();
		input.value = selectedHexXY;
		battleState.processActionInput(input);
	}

	private void commitTargetSelection() {
		hexGridRenderer.clearHilight();
		hexGridRenderer.clearHilight2();
		hexGridRenderer.reDrawColor();
		if (battleState.isTargetValid(selectedHexXY)) {
			EntityRenderer sourceRenderer = hexGridRenderer.getEntityRendererByName(battleState.getCurrentCombatant().name);
			Vector3 sourcePos = HexGridRenderer.getXYZPos(battleState.getCurrentCombatant().pos);
			Vector3 targetPos = HexGridRenderer.getXYZPos(selectedHexXY);
			//Debug.Log("Current action type: " + battleState.getCurrentAction().type);

			float animationDelay = 0;
			if (battleState.getCurrentAction().type == CombatActionType.MELEE_ATTACK) {
				animationDelay = animationManager.queueAnimation(new BumpAnimation(sourceRenderer.gameObject, targetPos, HexGridRenderer.CELL_WIDTH / 2)).getDuration() / 2;
			} else if (battleState.getCurrentAction().type == CombatActionType.RANGE_ATTACK) {
				animationDelay = animationManager.queueAnimation(new SimpleProjectileAnimation(hexGridRenderer.gameObject, sourcePos, targetPos)).getDuration();
			}
			List<CombatEffect> pendingEffects = battleState.determineCombatEffects(selectedHexXY);
			foreach (CombatEffect pendingEffect in pendingEffects) {
				//There's a smarter way to do this.
				EntityRenderer targetRenderer = hexGridRenderer.getEntityRendererByName(pendingEffect.target.name);
				if (pendingEffect.damage > 0) {
					animationManager.queueAnimation(
						//SimpleTextAnimation relies on worldspace position, so convert it here.
						new SimpleTextAnimation(
							hexGridRenderer.transform.TransformPoint(HexGridRenderer.getXYZPos(pendingEffect.target.pos)), 
							pendingEffect.damage.ToString(), 
							1, 
							animationDelay
						)
					);
					if (pendingEffect.damage + pendingEffect.target.damage >= pendingEffect.target.hitpoints) {
						animationManager.queueAnimation(
							new SimpleFadeOut(targetRenderer.gameObject, 2, animationDelay)
						);
					}
				} else {
					//Bump the opposite direction from the source of the attack.
					animationManager.queueAnimation(new BumpAnimation(targetRenderer.gameObject, sourcePos, -HexGridRenderer.CELL_WIDTH / 2, 0.5f, 0.1f));
				}
				
			}
		} else {
			//We consider this to be skipped input.
		}

		BattleInput input = new BattleInput();
		//input.combatantId = battleState.getCurrentCombatantId();
		input.value = selectedHexXY;
		battleState.processActionInput(input);
	}

	//----------------------------------------
	// UI Control handlers
	//----------------------------------------

	private bool dragGrid = false;

	private void MiddleClick_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
		bool isDown = obj.ReadValueAsButton();
		dragGrid = isDown;
	}

	private void ScrollWheel_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
		Vector2 delta = obj.ReadValue<Vector2>();
		Vector3 pos = new Vector3(0, delta.y / 10, 0);
		Camera.main.transform.position += pos;
	}

	private void Point_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
		if (dragGrid) {
			//TODO: find a better way to do this.
			Vector2 delta = UnityEngine.InputSystem.Mouse.current.delta.ReadValue();
			Vector3 pos = new Vector3(-delta.x, 0, -delta.y);
			Camera.main.transform.position += pos;
		} else {
			if (uiInputState == SELECT_CARD) {
				

			} else if (uiInputState == SELECT_MOVE) {
				Vector2 screenPos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
				Ray ray = Camera.main.ScreenPointToRay(screenPos);
				RaycastHit hit;
				if (Physics.Raycast(ray, out hit)) {
					HexGridRenderer hexGridRenderer = hit.transform.gameObject.GetComponent<HexGridRenderer>();
					if (hexGridRenderer != null) {
						Vector2Int hexPos = hexGridRenderer.getGridXYFromWorldPos(hit.point);
						if (!hexPos.Equals(selectedHexXY)) {
							//Debug.Log("mouse over hex: " + hexPos);
							selectedHexXY = hexPos;
							//Temporary:
							hilightSelectedHexXY();
							updateHexPathRenderer();
						}
					}
				}
			} else if (uiInputState == SELECT_TARGET) {
				Vector2 screenPos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
				Ray ray = Camera.main.ScreenPointToRay(screenPos);
				RaycastHit hit;
				if (Physics.Raycast(ray, out hit)) {
					HexGridRenderer hexGridRenderer = hit.transform.gameObject.GetComponent<HexGridRenderer>();
					if (hexGridRenderer != null) {
						Vector2Int hexPos = hexGridRenderer.getGridXYFromWorldPos(hit.point);
						if (!hexPos.Equals(selectedHexXY)) {
							//Debug.Log("mouse over hex: " + hexPos);
							selectedHexXY = hexPos;
							hexGridRenderer.clearHilight();
							if (hexGridRenderer.isHilighted2(hexPos)) {
								hexGridRenderer.setHilight(HexGrid.getXYsWithinRadius(hexPos.x, hexPos.y, battleState.getCurrentAction().radius));
							}
							hexGridRenderer.reDrawColor();
						}
					}
				}
			}
		}
	}

	private void Navigate_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
		//Debug.Log("action:" + obj.ToString());
		//UnityEngine.InputSystem.InputAction action = obj.action;
		//Debug.Log("action type: " + action.type);
		//Debug.Log("Type: " + obj.GetType());
		Vector2 value = obj.ReadValue<Vector2>();
		InputAction action = obj.action;
		string actionName = action.name;
		//Debug.Log("action: " + actionName);
		InputControl control = obj.control;
		string controlName = control.name;
		string controlShortName = control.shortDisplayName;
		//Debug.Log("control: " + controlName + " " + controlShortName);
		//Vector2 value2 = action.ReadValue<Vector2>();
		//Debug.Log("value: " + value);
		if (controlName == "dpad" || controlName == "rightArrow" || controlName == "leftArrow") {
			if (uiInputState == SELECT_CARD) {
				if (value.x < 0) {
					cardListDisplay.selectedIndex--;
				} else if (value.x > 0) {
					cardListDisplay.selectedIndex++;
				}
			} else if (uiInputState == SELECT_MOVE) {

			} else if (uiInputState == SELECT_TARGET) {

			}
		} else if (controlName == "w" || controlName == "a" || controlName == "s" || controlName == "d") {
			//Will have to fix this later...
			Vector3 pos = new Vector3(value.x, 0, value.y);
			Camera.main.transform.position += pos;
		}
	}

	private void Submit_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
		//Debug.Log("Submit " + obj.control.name); //A button.
		if (uiInputState == SELECT_CARD) {
			if (cardListDisplay.selectedIndex != -1) {
				commitCardSelection();
			}
		}

	}

	private void Cancel_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
		
	}

	private bool passSelected = false;

	private void Click_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
		bool isDown = obj.ReadValueAsButton();
		if (battleState.getCombatantsInTurnOrder().Count > 0) {
			//Debug.Log("Click performed " + getInputName() + " " + battleState.getCurrentCombatant().name);
		} else {
			Debug.Log("Click performed but combatants are all gone now. (Maybe this is a residual button release? Click is down: " + isDown);
			return;
		}
		Vector2 pos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
		if (uiInputState == SELECT_CARD) {
			if (isDown) {
				//Insert: if not deselecting the selected card...

				//This gonna have to be fixed later...
				if (isOverPassButton(pos)) {
					passSelected = true;
					passButton.OnPointerDown(new PointerEventData(null)); //new BaseEventData(EventSystem.current)
				}

				int selectedIndex = cardListDisplay.getCardIndexAtScreenPos(pos);

				if (selectedIndex != -1) {
					cardListDisplay.selectedIndex = selectedIndex;
					commitCardSelection();
				}
			} else {
				if (isOverPassButton(pos)) {
					if (passSelected) {
						//do something
						//Debug.Log("pass");
						BattleInput input = new BattleInput();
						//input.combatantId = battleState.getCurrentCombatantId();
						input.value = new Vector2Int(cardListDisplay.selectedIndex, 0);
						battleState.processActionInput(input);
					}
					passButton.OnPointerUp(new PointerEventData(null)); //new BaseEventData(EventSystem.current)
				} else {
					passButton.OnPointerUp(new PointerEventData(null)); //new BaseEventData(EventSystem.current)
				}
				passSelected = false;
			}
		} else if (uiInputState == SELECT_MOVE) {
			if (isInsideUI(pos)) {
				//Debug.Log("inside UI");
				return;
			}
			if (isDown) {
				Ray ray = Camera.main.ScreenPointToRay(pos);
				RaycastHit hit;
				if (Physics.Raycast(ray, out hit)) {
					HexGridRenderer hexGridRenderer = hit.transform.gameObject.GetComponent<HexGridRenderer>();
					if (hexGridRenderer != null) {
						Vector2Int hexPos = hexGridRenderer.getHexPosFromWorldPoint(hit.point);
						if (hexPathRenderer.pathPos.Count > 0) {
							selectedHexXY = hexPos;
							commitMoveSelection();
						}
					}
				}
			}
		} else if (uiInputState == SELECT_TARGET) {
			if (isInsideUI(pos)) {
				//Debug.Log("inside UI");
				return;
			}
			if (isDown) {
				Ray ray = Camera.main.ScreenPointToRay(pos);
				RaycastHit hit;
				if (Physics.Raycast(ray, out hit)) {
					HexGridRenderer hexGridRenderer = hit.transform.gameObject.GetComponent<HexGridRenderer>();
					if (hexGridRenderer != null) {
						Vector2Int hexPos = hexGridRenderer.getHexPosFromWorldPoint(hit.point);
						if (hexGridRenderer.isHilighted2(hexPos)) {
							//selectedHexXY = hexPathRenderer.pathPos[0];
							selectedHexXY = hexPos;
							commitTargetSelection();
						} else if (hexPos.Equals(battleState.getCurrentCombatant().pos)) {
							//Consider this to have been a cancelled input (ie skip the targeting).
							//This is a temporary input solution. There should be a better way to select null target.
							selectedHexXY = battleState.getCurrentCombatant().pos;
							commitTargetSelection();
						}
					}
				}
			}
		} else {
			//Debug.Log("Don't do anything.");
		}
	}

	private void RightClick_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
		
	}

	//----------------------------------------
	// Misc.
	//----------------------------------------

	private void updateHexPathRenderer() {
		List<Vector2Int> path = new List<Vector2Int>();
		try {
			HexNodePathMap pathMap = new HexNodePathMap(battleState.hexGrid, battleState);
			pathMap.setOrigin(battleState.getCurrentCombatant().pos);
			//Can only move within max range of the action.
			HexNode dest = pathMap.getClosestHexNodeTo(selectedHexXY, battleState.getCurrentAction().maxRange);
			while (dest != null) {
				path.Add(dest.hexPos);
				dest = (HexNode)dest.prevNode;
			}
		} catch (ArgumentOutOfRangeException e) {
			Debug.Log("ArgumentOutOfRange " + e.ActualValue + " - " + e.Message);
		}
		hexPathRenderer.pathPos = path;
		hexPathRenderer.updateOnDemand();
	}

	private void hilightSelectedHexXY() {
		hexGridRenderer.clearHilight();
		hexGridRenderer.setHilight(selectedHexXY);
		hexGridRenderer.reDrawColor();
	}

	private string getInputStateName() {
		switch (uiInputState) {
			case SELECT_CARD: return "Select Card";
			case SELECT_MOVE: return "Move";
			case SELECT_TARGET: return "Select Target";
			default: return "unknown";
		}
	}

	/// <summary>
	/// Brute force method for testing if a screen position is inside the UI bounds.
	/// </summary>
	/// <param name="screenPos"></param>
	/// <returns></returns>
	private bool isInsideUI(Vector2 screenPos) {
		bool inside = cardListDisplay.isActiveAndEnabled && RectTransformUtility.RectangleContainsScreenPoint(
			cardListDisplay.gameObject.GetComponent<RectTransform>(), screenPos
		);

		inside = inside || (selectedCardRenderer.isActiveAndEnabled && RectTransformUtility.RectangleContainsScreenPoint(
			selectedCardRenderer.gameObject.GetComponent<RectTransform>(), screenPos
		));

		return inside;
	}

	private bool isOverPassButton(Vector2 screenPos) {
		return RectTransformUtility.RectangleContainsScreenPoint(
			passButton.gameObject.GetComponent<RectTransform>(), screenPos
		);
	}

	private string getInputName() {
		switch (uiInputState) {
			case DISABLED: return "DISABLED";
			case SELECT_CARD: return "SELECT CARD";
			case SELECT_MOVE: return "SELECT MOVE";
			case SELECT_TARGET: return "SELECT TARGET";
			default: return "?";
		}
	}

}
