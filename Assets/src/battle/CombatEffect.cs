/// <summary>
/// Combat effects are the result of combat actions being applied to a target.
/// They are used to modify battle state.
/// This class is being made simple to start with and covers damage.
/// </summary>
public class CombatEffect {
	public int combatantId;
	public int damage;
	//TODO: it will be possible to force discards here as well... in case of reaction cards (and possibly also for normal selection)
}
