using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// For the sake of simplicity, the deck will contain lists of cards in various states instead of messing with id's.
/// </summary>
public class Deck {

	private static List<Card> shuffleCards(List<Card> cards) {
		int currentIndex = cards.Count;
		int randomIndex;
		Card placeHolder;
			
		// While there remain elements to shuffle...
		while (currentIndex != 0) {
			// Pick a remaining element...
			//randomIndex = Math.floor(PRNG.getInstance().random() * currentIndex);
			randomIndex = GameState.PRNG.randomInt() % currentIndex;
			currentIndex -= 1;
				
			// And swap it with the current element.
			placeHolder = cards[currentIndex];
			cards[currentIndex] = cards[randomIndex];
			cards[randomIndex] = placeHolder;
		}
		return cards;
	}

	private int handSize;

	private List<Card> drawPile = new List<Card>();
	private List<Card> discardPile = new List<Card>();
	private List<Card> hand = new List<Card>();
	private List<Card> removedCards = new List<Card>();

	public Deck(List<Card> cards, int handSize=4) {
		drawPile.AddRange(cards);
		shuffleCards(drawPile);
		this.handSize = handSize;
	}

	public List<Card> getHand() {
		return hand;
	}

	/// <summary>
	/// Shuffle the discard pile into the draw pile.
	/// </summary>
	public void reshuffle() {
		while (discardPile.Count > 0) {
			int i = GameState.PRNG.randomInt() % discardPile.Count;
			drawPile.Add(discardPile[i]);
			discardPile.RemoveAt(i);
		}
	}

	public int getDrawsRemaining() {
		return drawPile.Count;
	}

	/// <summary>
	/// Draw up to the hand limit.
	/// </summary>
	public void drawHand() {
		if (hand.Count < handSize) {
			draw(handSize - hand.Count);
		}
	}

	/// <summary>
	/// Draw a number of cards from the draw deck.
	/// Cannot draw more cards than are currently in the draw pile.
	/// </summary>
	/// <param name="count">The number of cards to draw.</param>
	/// <returns>The actual number of cards drawn.</returns>
	public int draw(int count=1) {
		count = Mathf.Min(count, drawPile.Count);
		hand.AddRange(drawPile.GetRange(0, count));
		drawPile.RemoveRange(0, count);
		return count;
	}

	/// <summary>
	/// Discard a card from the hand.
	/// </summary>
	/// <param name="card">The card to discard</param>
	public void discard(Card card) {
		hand.Remove(card);
		discardPile.Add(card);
	}

	public int getDiscardCount() {
		return discardPile.Count;
	}

	/// <summary>
	/// Remove a card from the hand entirely from play.
	/// </summary>
	/// <param name="card">The card to remove from the deck</param>
	public void removeCard(Card card) {
		hand.Remove(card);
		removedCards.Add(card);
	}
}
