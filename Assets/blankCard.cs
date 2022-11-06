using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class blankCard : MonoBehaviour {

	// Use this for initialization
	public new KMAudio audio;
	public KMBombInfo bomb;
	static int moduleIdCounter = 1;
	int moduleId;
	public KMBombModule module;
	public KMSelectable clearButton;
	public KMSelectable[] points;
	public AudioClip solveSFX;
	public Transform markedLine;
	public Transform[] lines;
	public LineRenderer lineRenderer;
	public MeshCollider canvas;
	public Transform canvasTrans;
	public Material[] cards;
	public MeshRenderer cardForm;
	public Material white;
	private int numConsecutiveLines = 0;
	private int numLineCounter = 0;
	private int submitResult = -1;
	private ArrayList connectedLines = new ArrayList();
	private int[] lastConnection = new int[2];
	private int[] index = { 0, 1, 2, 3 };
	private int[] card = new int[2];
	private float[] Xpos = { 0.0222f, 0.035f, 0.04764f, 0.06044f };
	private float[] Ypos = { 0.0171f, 0.00486f, -0.0074f, -0.0196f, -0.03186f };
	private float[][][] connectedRange = {
		new float[][]{
		new float[]{ 0.11f, 0.19f, 0.05f, 0.13f },
		new float[]{ 0.79f, 0.90f, 0.87f, 0.94f }
		},

		new float[][]{
		new float[]{ 0.79f, 0.90f, 0.05f, 0.13f },
		new float[]{ 0.10f, 0.20f, 0.87f, 0.94f }
		},
		new float[][]{
		new float[]{ 0.79f, 0.90f, 0.05f, 0.13f },
		new float[]{ 0.11f, 0.19f, 0.87f, 0.94f }
		},
		new float[][]{
		new float[]{ 0.11f, 0.19f, 0.05f, 0.13f },
		new float[]{ 0.79f, 0.90f, 0.87f, 0.94f },
		}
	};
	private int[][] connectPoint =
	{
		new int[]{ 1, 2 },
		new int[]{ 0, 3 },
		new int[]{ 0, 3 },
		new int[]{ 1, 2 }
	};

	private int[][] connectLine =
	{
		new int[]{ 0, 1 },
		new int[]{ 0, 2 },
		new int[]{ 1, 3 },
		new int[]{ 2, 3 }
	};
	void Start()
	{
		moduleId = moduleIdCounter++;
		card[0] = UnityEngine.Random.Range(0, 13);
		markedLine.transform.localPosition = new Vector3(Xpos[card[0] % 4], 0.01559f, Ypos[card[0] / 4]);
		card[1] = UnityEngine.Random.Range(0, 4);
		int angle = 45 * card[1];
		markedLine.transform.Rotate(Vector3.up, angle);
		Debug.LogFormat("[Blank Card #{0}] Marked Card: {1} of {2}", moduleId, new string[] { "Ace", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Jack", "Queen", "King" }[card[0]], new string[] { "Spades", "Hearts", "Clubs", "Diamonds" }[card[1]]);
		foreach (int i in index)
		{
			points[i].OnInteract = delegate { StartCoroutine(connecting(i)); return false; };
			points[i].OnInteractEnded = delegate { };
		}
		clearButton.OnInteract = delegate {
			clearButton.AddInteractionPunch();
			audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform); reset();
			return false;
		};
		lineRenderer.positionCount = 2;

		cards[0].color = new Color(0f, 0f, 0f);
		lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
		lineRenderer.startColor = Color.white;
		lineRenderer.endColor = Color.white;
	}
	private IEnumerator connecting(int point)
	{
		yield return null;
		//float forward = Vector3.Dot(Camera.main.transform.TransformVector(Vector3.forward), module.transform.TransformVector(Vector3.forward));
		//Debug.LogFormat("[Blank Card #{0}] {1}", moduleId, forward);

		float scalar = transform.lossyScale.x;
		lineRenderer.startWidth = 0.01f * scalar;
		lineRenderer.endWidth = 0.01f * scalar;
		lineRenderer.SetPosition(0, new Vector3(points[point].transform.localPosition.x, 1f, points[point].transform.localPosition.z));


		int connectedPoint = -1;
		while (TwitchPlaysActive ? DrawingTP : Input.GetMouseButton(0))
		{

			Ray ray = Camera.main.ScreenPointToRay(TwitchPlaysActive ? DrawTPPos : Input.mousePosition);
			RaycastHit hit;
			canvas.Raycast(ray, out hit, 100.0f);
			Vector2 v = hit.textureCoord;
			lineRenderer.SetPosition(1, new Vector3(points[point].transform.localPosition.x, 1f, points[point].transform.localPosition.z));
			while (v.x == 0 && v.y == 0)
			{
				
				ray = Camera.main.ScreenPointToRay(TwitchPlaysActive ? DrawTPPos : Input.mousePosition);
				canvas.Raycast(ray, out hit, 100.0f);
				v = hit.textureCoord;
				yield return new WaitForSeconds(0.01f);
			}
			if (TwitchPlaysActive ? DrawingTP : Input.GetMouseButton(0))
				lineRenderer.SetPosition(1, new Vector3((1f - v.x) - 0.5f, 1f, (1f - v.y) - 0.5f));
			
			//Debug.LogFormat("[Blank Card #{0}] {1} {2}", moduleId, v.x, v.y);
			//Debug.LogFormat("[Blank Card #{0}] {1} ", moduleId, lineRenderer.startWidth);
			if (v.x >= connectedRange[point][0][0] && v.x <= connectedRange[point][0][1] && v.y >= connectedRange[point][0][2] && v.y <= connectedRange[point][0][3] && !(connectedLines.Contains(connectLine[point][0])))
			{
				lines[connectLine[point][0]].transform.localPosition = new Vector3(lines[connectLine[point][0]].transform.localPosition.x, 0.55f, lines[connectLine[point][0]].transform.localPosition.z);
				connectedPoint = connectPoint[point][0];
				connectedLines.Add(connectLine[point][0]);
				break;
			}
			else if (v.x >= connectedRange[point][1][0] && v.x <= connectedRange[point][1][1] && v.y >= connectedRange[point][1][2] && v.y <= connectedRange[point][1][3] && !(connectedLines.Contains(connectLine[point][1])))
			{
				lines[connectLine[point][1]].transform.localPosition = new Vector3(lines[connectLine[point][1]].transform.localPosition.x, 0.55f, lines[connectLine[point][1]].transform.localPosition.z);
				connectedPoint = connectPoint[point][1];
				connectedLines.Add(connectLine[point][1]);
				break;
			}
			
			yield return new WaitForSeconds(0.01f);
		}
		lineRenderer.SetPosition(0, new Vector3(0f, 0f, 0f));
		lineRenderer.SetPosition(1, new Vector3(0f, 0f, 0f));
		points[point].OnInteractEnded();
		if (connectedPoint != -1)
		{
			updateInteractions();
			lastConnection[0] = point;
			lastConnection[1] = connectedPoint;
			numLineCounter++;
			if(points[connectedPoint].OnInteract != null)
				points[connectedPoint].OnInteract();
			else
			{
				numConsecutiveLines = numLineCounter;
				numLineCounter = 0;
				if (connectedLines.Count == 4)
					StartCoroutine(formCard());
			}
		}
		else
		{
			numConsecutiveLines = numLineCounter;
			numLineCounter = 0;
		}
	}
	private void updateInteractions()
	{
		if (connectedLines.Contains(0) && connectedLines.Contains(1))
			points[0].OnInteract = null;
		if (connectedLines.Contains(0) && connectedLines.Contains(2))
			points[1].OnInteract = null;
		if (connectedLines.Contains(1) && connectedLines.Contains(3))
			points[2].OnInteract = null;
		if (connectedLines.Contains(2) && connectedLines.Contains(3))
			points[3].OnInteract = null;
	}
	private IEnumerator formCard()
	{
		clearButton.OnInteract = null;
		Debug.LogFormat("[Blank Card #{0}] Lines: {1} {2} {3} {4}", moduleId, connectedLines[0], connectedLines[1], connectedLines[2], connectedLines[3]);
		Debug.LogFormat("[Blank Card #{0}] Last Connection: {1} {2}", moduleId, lastConnection[0], lastConnection[1]);
		int suit = (int)connectedLines[0];
		int rank;
		if(numConsecutiveLines == 4)
			rank = 12;
		else
		{
			int[] ranks;
			switch(Array.IndexOf(new int[] { 0, 2, 3, 1 }, (int)connectedLines[1]) - Array.IndexOf(new int[] { 0, 2, 3, 1 }, suit))
			{
				case 1:
				case -3:
					ranks = new int[] { 0, 1, 2, 3};
					break;
				case 2:
				case -2:
					ranks = new int[] { 4, 5, 6, 7 };
					break;
				default:
					ranks = new int[] { 8, 9, 10, 11 };
					break;
			}
			ArrayList vals = new ArrayList() { 0, 2, 3, 1 };
			vals.Remove(suit);
			int firstLine = (int)vals[(vals.IndexOf(connectedLines[1]) + 1) % 3];
			if (firstLine == (int)connectedLines[2])
				ranks = new int[] { ranks[0], ranks[2] };
			else
				ranks = new int[] { ranks[1], ranks[3] };
			switch(lastConnection[0] + "" + lastConnection[1])
			{
				case "10":
				case "32":
				case "02":
				case "13":
					rank = ranks[0];
					break;
				default:
					rank = ranks[1];
					break;
			}
		}
		Debug.LogFormat("[Blank Card #{0}] Card Formed: {1} of {2}", moduleId, new string[] { "Ace", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Jack", "Queen", "King" }[rank], new string[] { "Spades", "Hearts", "Clubs", "Diamonds" }[suit]);
		cardForm.material = cards[(rank * 4) + suit];
		if (card[0] == rank && card[1] == suit)
			submitResult = 0;
		else
			submitResult = 1;

		yield return new WaitForSeconds(1f);
		for(float aa = 0; aa <= 1.0f; aa += 0.01f)
		{
			
			cardForm.material.color = new Color(aa, aa, aa);
			yield return new WaitForSeconds(0.01f);
		}
		yield return new WaitForSeconds(1f);
		if (submitResult == 0)
		{
			audio.PlaySoundAtTransform(solveSFX.name, transform);
			module.HandlePass();
		}
		else
		{
			module.HandleStrike();
			cardForm.material.color = new Color(0f, 0f, 0f);
			reset();
		}
		submitResult = -1;
	}
	private void reset()
	{
		foreach (int i in index)
		{
			points[i].OnInteract = delegate { StartCoroutine(connecting(i)); return false; };
			lines[i].transform.localPosition = new Vector3(lines[i].transform.localPosition.x, -1f, lines[i].transform.localPosition.z);
		}
		clearButton.OnInteract = delegate {
			clearButton.AddInteractionPunch();
			audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform); reset();
			return false;
		};
		numConsecutiveLines = 0;
		connectedLines.Clear();
	}
#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} draw 1 2 4 [Draws lines starting at the first specified dot and then dragging to all other specified dots in the order given. Dots are 1-4 in reading order.] | !{0} clear [Presses the clear button.]";
#pragma warning restore 414
	private bool TwitchPlaysActive;
	private bool DrawingTP;
	private bool Autosolved;
	private Vector3 DrawTPPos;
	IEnumerator ProcessTwitchCommand(string command)
	{
		if (Regex.IsMatch(command, @"^\s*clear\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			yield return null;
			clearButton.OnInteract();
			yield break;
		}
		string[] parameters = command.Split(' ');
		if (Regex.IsMatch(parameters[0], @"^\s*draw\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
			if (parameters.Length == 1 || parameters.Length == 2)
				yield return "sendtochaterror Please specify at least 2 dots to draw lines!";
            else
			{
				for (int i = 1; i < parameters.Length; i++)
				{
					int temp = -1;
					if (!int.TryParse(parameters[i], out temp))
                    {
						yield return "sendtochaterror!f The specified dot '" + parameters[i] + "' is invalid!";
						yield break;
                    }
					if (temp < 1 || temp > 4)
                    {
						yield return "sendtochaterror The specified dot '" + parameters[i] + "' is invalid!";
						yield break;
					}
				}
				if (points[int.Parse(parameters[1]) - 1].OnInteract == null)
                {
					yield return "sendtochaterror Lines can no longer be drawn from the dot '" + parameters[1] + "'!";
					yield break;
				}
				if (submitResult != -1)
                {
					yield return "sendtochaterror Lines cannot be drawn while the card is forming!";
					yield break;
				}
				yield return null;
				DrawingTP = true;
				for (int i = 1; i < parameters.Length - 1; i++)
				{
					if (i == 1)
                    {
						DrawTPPos = Camera.main.WorldToScreenPoint(points[int.Parse(parameters[1]) - 1].transform.position);
						points[int.Parse(parameters[1]) - 1].OnInteract();
					}
					Vector3 origPos = DrawTPPos;
					float t = 0f;
					int prevConCt = connectedLines.Count;
					while (prevConCt == connectedLines.Count)
                    {
						yield return null;
						DrawTPPos = Vector3.Lerp(origPos, Camera.main.WorldToScreenPoint(points[int.Parse(parameters[i + 1]) - 1].transform.position), t);
						t += Time.deltaTime * 2f;
						if (t > 2f)
							break;
                    }
				}
				DrawingTP = false;
				if (submitResult == 0 && !Autosolved)
					yield return "solve";
				else if (submitResult == 1 && !Autosolved)
					yield return "strike";
			}
        }
	}
	IEnumerator TwitchHandleForcedSolve()
    {
		Autosolved = true;
		while (DrawingTP) yield return null;
		if (submitResult == 1)
        {
			StopAllCoroutines();
			module.HandlePass();
		}
        else if (submitResult == -1)
        {
			if (connectedLines.Count > 0)
            {
				clearButton.OnInteract();
				yield return new WaitForSeconds(.1f);
            }
			if (card[0] == 12 && card[1] == 0)
				yield return ProcessTwitchCommand("draw 1 2 4 3 1");
			else if (card[0] == 12 && card[1] == 1)
				yield return ProcessTwitchCommand("draw 1 3 4 2 1");
			else if (card[0] == 12 && card[1] == 2)
				yield return ProcessTwitchCommand("draw 2 4 3 1 2");
			else if (card[0] == 12 && card[1] == 3)
				yield return ProcessTwitchCommand("draw 3 4 2 1 3");
            else
            {
				List<int> usedIndexes = new List<int>();
				for (int i = 0; i < 4; i++)
                {
					int drawIndex = -1;
					bool reverse = false;
					if (i == 0)
                    {
						drawIndex = new int[] { 0, 3, 1, 2 }[card[1]];
						usedIndexes.Add(drawIndex);
					}
					else if (i == 1)
                    {
						drawIndex = card[0] / 4 + 1 + usedIndexes[usedIndexes.Count - 1];
						drawIndex %= 4;
						usedIndexes.Add(drawIndex);
					}
					else if (i == 2)
					{
						for (int j = 0; j < ((card[0] % 2 == 0) ? 1 : 2); j++)
                        {
							if (j == 0)
								drawIndex = (usedIndexes[usedIndexes.Count - 1] + 1) % 4;
							else
								drawIndex = (drawIndex + 1) % 4;
							while (usedIndexes.Contains(drawIndex))
							{
								drawIndex++;
								if (drawIndex > 3)
									drawIndex = 0;
							}
						}
						usedIndexes.Add(drawIndex);
					}
                    else
                    {
						for (int j = 0; j < 4; j++)
                        {
							if (!usedIndexes.Contains(j))
							{
								drawIndex = j;
								break;
							}
                        }
						if ((card[0] % 4) < 2) reverse = true;
                    }
					yield return ProcessTwitchCommand("draw " + GetTPLineString(drawIndex, reverse));
					yield return new WaitForSeconds(.1f);
				}
			}
		}
		while (submitResult != -1) yield return true;
    }
	string GetTPLineString(int lineIndex, bool reverse)
    {
		string result;
		if (lineIndex == 0)
			result = "1 2";
		else if (lineIndex == 1)
			result = "4 2";
		else if (lineIndex == 2)
			result = "3 4";
		else
			result = "3 1";
		if (reverse)
			return result.Reverse().Join("");
		else
			return result;
	}
}
