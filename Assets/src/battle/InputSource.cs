public interface InputSource {
	/// <summary>
	/// It is the responsibility of this method to determine the input from the current battle state.
	/// Then it must send this input back to the battle state which will again call this method to continue.
	/// </summary>
	/// <param name="battleState"></param>
	void processNextAction(BattleState battleState);
}
