﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Deck : MonoBehaviour {
	//Suits
	public Sprite suitClub;
	public Sprite suitDiamond;
	public Sprite suitHeart;
	public Sprite suitSpade;
	
	public Sprite[] faceSprites;
	public Sprite[] rankSprites;
	
	public Sprite cardBack;
	public Sprite cardBackGold;
	public Sprite cardFront;
	public Sprite cardFrontGold;
	
	
	// Prefabs
	public GameObject prefabSprite;
	public GameObject prefabCard;

	public bool _____________________;

	public PT_XMLReader					xmlr;
	// add from p 569
	public List<string>					cardNames;
	public List<Card>					cards;
	public List<Decorator>				decorators;
	public List<CardDefinition>			cardDefs;
	public Transform					deckAnchor;
	public Dictionary<string, Sprite>	dictSuits;


	// called by Prospector when it is ready
	public void InitDeck(string deckXMLText) {
		// from page 576
		if( GameObject.Find("_Deck") == null) {
			GameObject anchorGO = new GameObject("_Deck");
			deckAnchor = anchorGO.transform;
		}
		
		// init the Dictionary of suits
		dictSuits = new Dictionary<string, Sprite>() {
			{"C", suitClub},
			{"D", suitDiamond},
			{"H", suitHeart},
			{"S", suitSpade}
		};
		
		
		
		// -------- end from page 576
		ReadDeck (deckXMLText);
		MakeCards();
	}


	// ReadDeck parses the XML file passed to it into Card Definitions
	public void ReadDeck(string deckXMLText)
	{
		xmlr = new PT_XMLReader ();
		xmlr.Parse (deckXMLText);

		// print a test line
		string s = "xml[0] decorator [0] ";
		s += "type=" + xmlr.xml ["xml"] [0] ["decorator"] [0].att ("type");
		s += " x=" + xmlr.xml ["xml"] [0] ["decorator"] [0].att ("x");
		s += " y=" + xmlr.xml ["xml"] [0] ["decorator"] [0].att ("y");
		s += " scale=" + xmlr.xml ["xml"] [0] ["decorator"] [0].att ("scale");
		print (s);
		
		//Read decorators for all cards
		// these are the small numbers/suits in the corners
		decorators = new List<Decorator>();
		// grab all decorators from the XML file
		PT_XMLHashList xDecos = xmlr.xml["xml"][0]["decorator"];
		Decorator deco;
		for (int i=0; i<xDecos.Count; i++) {
			// for each decorator in the XML, copy attributes and set up location and flip if needed
			deco = new Decorator();
			deco.type = xDecos[i].att ("type");
			deco.flip = (xDecos[i].att ("flip") == "1");   // too cute by half - if it's 1, set to 1, else set to 0
			deco.scale = float.Parse (xDecos[i].att("scale"));
			deco.loc.x = float.Parse (xDecos[i].att("x"));
			deco.loc.y = float.Parse (xDecos[i].att("y"));
			deco.loc.z = float.Parse (xDecos[i].att("z"));
			decorators.Add (deco);
		}
		
		// read pip locations for each card rank
		// read the card definitions, parse attribute values for pips
		cardDefs = new List<CardDefinition>();
		PT_XMLHashList xCardDefs = xmlr.xml["xml"][0]["card"];
		
		for (int i=0; i<xCardDefs.Count; i++) {
			// for each carddef in the XML, copy attributes and set up in cDef
			CardDefinition cDef = new CardDefinition();
			cDef.rank = int.Parse(xCardDefs[i].att("rank"));
			
			PT_XMLHashList xPips = xCardDefs[i]["pip"];
			if (xPips != null) {			
				for (int j = 0; j < xPips.Count; j++) {
					deco = new Decorator();
					deco.type = "pip";
					deco.flip = (xPips[j].att ("flip") == "1");   // too cute by half - if it's 1, set to 1, else set to 0
					
					deco.loc.x = float.Parse (xPips[j].att("x"));
					deco.loc.y = float.Parse (xPips[j].att("y"));
					deco.loc.z = float.Parse (xPips[j].att("z"));
					if(xPips[j].HasAtt("scale") ) {
						deco.scale = float.Parse (xPips[j].att("scale"));
					}
					cDef.pips.Add (deco);
				} // for j
			}// if xPips
			
			// if it's a face card, map the proper sprite
			// foramt is ##A, where ## in 11, 12, 13 and A is letter indicating suit
			if (xCardDefs[i].HasAtt("face")){
				cDef.face = xCardDefs[i].att ("face");
			}
			cardDefs.Add (cDef);
		} // for i < xCardDefs.Count
	} // ReadDeck
	
	public CardDefinition GetCardDefinitionByRank(int rnk) {
		foreach(CardDefinition cd in cardDefs) {
			if (cd.rank == rnk) {
					return(cd);
			}
		} // foreach
		return (null);
	}//GetCardDefinitionByRank
	
	
	public void MakeCards() {
		// cardNames will be the names of cards to build
		// Each suit goes from 1 to 13 (e.g., C1 to C13 for Clubs)
		cardNames = new List<string>();
		string[] letters = new string[] {"C","D","H","S"};
		foreach (string s in letters) {
			for (int i=0; i<13; i++) {
				cardNames.Add(s+(i+1));
			}
		}
		// Make a List to hold all the cards
		cards = new List<Card>();
		// Several variables that will be reused several times
		Sprite tS = null;
		GameObject tGO = null;
		SpriteRenderer tSR = null;
		// Iterate through all of the card names that were just made
		for (int i=0; i<cardNames.Count; i++) {
			// Create a new Card GameObject
			GameObject cgo = Instantiate(prefabCard) as GameObject;
			// Set the transform.parent of the new card to the anchor.
			cgo.transform.parent = deckAnchor;
			Card card = cgo.GetComponent<Card>(); // Get the Card Component
			// This just stacks the cards so that they're all in nice rows
			cgo.transform.localPosition = new Vector3( (i%13)*3, i/13*4, 0 );
			// Assign basic values to the Card
			card.name = cardNames[i];
			card.suit = card.name[0].ToString();
			card.rank = int.Parse( card.name.Substring(1) );
			if (card.suit == "D" || card.suit == "H") {
				card.colS = "Red";
				card.color = Color.red;
			}
			// Pull the CardDefinition for this card
			card.def = GetCardDefinitionByRank(card.rank);
			// Add Decorators
			foreach( Decorator deco in decorators ) {
				if (deco.type == "suit") {
					// Instantiate a Sprite GameObject
					tGO = Instantiate( prefabSprite ) as GameObject;
					// Get the SpriteRenderer Component
					tSR = tGO.GetComponent<SpriteRenderer>();
					// Set the Sprite to the proper suit
					tSR.sprite = dictSuits[card.suit];
				} else { //if it's not a suit, it's a rank deco
					tGO = Instantiate( prefabSprite ) as GameObject;
					tSR = tGO.GetComponent<SpriteRenderer>();
					// Get the proper Sprite to show this rank
					tS = rankSprites[ card.rank ];
					// Assign this rank Sprite to the SpriteRenderer
					tSR.sprite = tS;
					// Set the color of the rank to match the suit
					tSR.color = card.color;
				}
				// Make the deco Sprites render above the Card
				tSR.sortingOrder = 1;
				// Make the decorator Sprite a child of the Card
				tGO.transform.parent = cgo.transform;
				// Set the localPosition based on the location from DeckXML
				tGO.transform.localPosition = deco.loc;
				// Flip the decorator if needed
				if (deco.flip) {
					// An Euler rotation of 180° around the Z-axis will flip it
					tGO.transform.rotation = Quaternion.Euler(0,0,180);
				}
				// Set the scale to keep decos from being too big
				if (deco.scale != 1) {
					tGO.transform.localScale = Vector3.one * deco.scale;
				}
				// Name this GameObject so it's easy to find
				tGO.name = deco.type;
				// Add this deco GameObject to the List card.decoGOs
				card.decoGOs.Add(tGO);
			}
			// Add Pips
			// For each of the pips in the definition
			foreach( Decorator pip in card.def.pips ) {
				// Instantiate a Sprite GameObject
				tGO = Instantiate( prefabSprite ) as GameObject;
				// Set the parent to be the card GameObject
				tGO.transform.parent = cgo.transform;
				// Set the position to that specified in the XML
				tGO.transform.localPosition = pip.loc;
				// Flip it if necessary
				if (pip.flip) {
					tGO.transform.rotation = Quaternion.Euler(0,0,180);
				}
				// Scale it if necessary (only for the Ace)
				if (pip.scale != 1) {
					tGO.transform.localScale = Vector3.one * pip.scale;
				}
				// Give this GameObject a name
				tGO.name = "pip";
				// Get the SpriteRenderer Component
				tSR = tGO.GetComponent<SpriteRenderer>();
				// Set the Sprite to the proper suit
				tSR.sprite = dictSuits[card.suit];
				// Set sortingOrder so the pip is rendered above the Card_Front
				tSR.sortingOrder = 1;
				// Add this to the Card's list of pips
				card.pipGOs.Add(tGO);
			}
			// Handle Face Cards
			if (card.def.face != "") { // If this has a face in card.def
				tGO = Instantiate( prefabSprite ) as GameObject;
				tSR = tGO.GetComponent<SpriteRenderer>();
				// Generate the right name and pass it to GetFace()
				tS = GetFace( card.def.face+card.suit );
				tSR.sprite = tS; // Assign this Sprite to tSR
				tSR.sortingOrder = 1; // Set the sortingOrder
				tGO.transform.parent = card.transform;
				tGO.transform.localPosition = Vector3.zero;
				tGO.name = "face";
			}
			// Add Card Back
			// The Card_Back will be able to cover everything else on the Card
			tGO = Instantiate( prefabSprite ) as GameObject;
			tSR = tGO.GetComponent<SpriteRenderer>();
			tSR.sprite = cardBack;
			tGO.transform.parent = card.transform;
			tGO.transform.localPosition = Vector3.zero;
			// This is a higher sortingOrder than anything else
			tSR.sortingOrder = 2;
			tGO.name = "back";
			card.back = tGO;
			// Default to face-up
			card.faceUp = false; // Use the property faceUp of Card
			// Add the card to the deck
			cards.Add (card);
		}
	}
	// This is the closing brace for MakeCards()
	// Find the proper face card Sprite
	public Sprite GetFace(string faceS) {
		foreach (Sprite tS in faceSprites) {
			// If this Sprite has the right name...
			if (tS.name == faceS) {
				// ...then return the Sprite
				return( tS );
			}
		}
		// If nothing can be found, return null
		return( null );
	}// makeCards
	// Shuffle the Cards in Deck.cards
	static public void Shuffle(ref List<Card> oCards) { // 1
		// Create a temporary List to hold the new shuffle order
		List<Card> tCards = new List<Card>();
		int ndx; // This will hold the index of the card to be moved
		tCards = new List<Card>(); // Initialize the temporary List
		// Repeat as long as there are cards in the original List
		while (oCards.Count > 0) {
			// Pick the index of a random card
			ndx = Random.Range(0,oCards.Count);
			// Add that card to the temporary List
			tCards.Add (oCards[ndx]);
			// And remove that card from the original List
			oCards.RemoveAt(ndx);
		}
		// Replace the original List with the temporary List
		oCards = tCards;
		// Because oCards is a reference variable, the original that was
		// passed in is changed as well.
	}
	
} // Deck class
