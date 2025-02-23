using Meta.WitAi;
using Meta.WitAi.Configuration;
using Meta.WitAi.Data;
using Meta.WitAi.Events;
// WitRequest is not needed
using UnityEngine;
using System.Collections;
using Meta.WitAi.Json;

public class MoldFruitsProcessor : MonoBehaviour
{
	public Wit wit;
	public GameObject fruitsParent;
	public float listenInterval = 0.5f;

	void Start()
	{
		if (wit == null)
		{
			Debug.LogError("Wit component not assigned!");
			return;
		}
		Debug.Log("Start");

		wit.VoiceEvents.OnFullTranscription.AddListener(HandleTranscription);
		wit.VoiceEvents.OnResponse.AddListener(HandleWitResponse);
		wit.VoiceEvents.OnError.AddListener(HandleWitError);
		StartCoroutine(ContinuousListen());
	}

	private IEnumerator ContinuousListen()
	{
		while (true)
		{
			if (wit != null && !wit.Active)
			{
				wit.Activate();
				Debug.Log("Listening");
			}
			yield return new WaitForSeconds(listenInterval);
		}
	}

	private void HandleTranscription(string transcript)
	{
		Debug.Log("Transcription: " + transcript);
	}

	private void HandleWitResponse(WitResponseNode response)
	{
		Debug.Log("Response: " +response.ToString());

		// Correct way to check for a valid response:
		if (response != null && response["text"] != null)
		{
			Debug.Log("Reached Handle Wit inside if");
			// Check for intents
			if (response["intents"] != null && response["intents"].Count > 0)
			{
				string intent = response["intents"][0]["name"].Value;
				Debug.Log("Intent: " + intent);

				if (intent == "mold_fruit")
				{

					HandleMoldFruits(response);
				}
				else
				{
					Debug.LogWarning("Unknown intent: " + intent);
				}
			}
			else
			{
				Debug.LogWarning("No intents found in Wit response.");
			}
		}
		else
		{
			// Handle null or invalid response (e.g., network error, Wit.ai service down)
			Debug.LogError("Invalid Wit response received.");
		}
	}

	private void HandleWitError(string error, string message)
	{
		Debug.LogError($"Wit.ai Error: {error} - {message}");
	}

	private void HandleMoldFruits(WitResponseNode response)
	{
		if (fruitsParent != null)
		{
			foreach (Transform fruitTransform in fruitsParent.transform)
			{
				GameObject fruitObject = fruitTransform.gameObject;
				Debug.Log("Molding fruit:  " + fruitObject.name);
				fruitObject.transform.localScale = new Vector3(1.2f, 0.8f, 1.2f);
			}
		}
		else
		{
			Debug.LogError("Fruits parent object not assigned!");
		}
	}

	private void OnDestroy()
	{
		if (wit != null)
		{
			wit.VoiceEvents.OnFullTranscription.RemoveListener(HandleTranscription);
			wit.VoiceEvents.OnResponse.RemoveListener(HandleWitResponse);
			wit.VoiceEvents.OnError.RemoveListener(HandleWitError);
		}
	}
}