/// <summary>
/// Combat effects are the result of combat actions being applied to a target.
/// They are used to modify battle state.
/// This class is being made simple to start with and covers damage.
/// </summary>
public class CombatEffect {
	public BattlefieldEntity target;
	public int damage;
	public int harassment;

	//Other penalty/bonus modifiers to be added.

	//TODO: it will be possible to force discards here as well... in case of reaction cards (and possibly also for normal selection)
}
