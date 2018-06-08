using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using System;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using IBM.Watson.DeveloperCloud.Services.Assistant.v1;
using IBM.Watson.DeveloperCloud.Utilities;
using IBM.Watson.DeveloperCloud.Services.TextToSpeech.v1;
using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Connection;

public class Photo : MonoBehaviour {
    public string txt;
    byte[] imageByteArray;
private const string CLOUD_NAME = "dylrioik3";
    private const string MICROSOFT_VISION_KEY = "6714dabe90b44cc4a60166a1088d7244";
    private const string MICROSOFT_VISION_URL = "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0/describe?maxCandidates=1";
    private GameObject status;
    private GameObject statusbg;
    private const string UPLOAD_PRESET_NAME = "qylb43yg";
private const string CLOUDINARY_API_KEY = "284551392584861";
private const string CLOUDINARY_SIGNATURE = "K5IE-CzFZS5HCZw_DHz5G6MVQVo";
private string imageURl;
private GameObject scanningObject;
public GameObject buttonObject;
public GameObject t1;
public GameObject t2;
    public GameObject wordBg1;
    public GameObject meaningBg1;
private const string BASE_URL = "http://www.google.com/searchbyimage?hl=ru&image_url=";
private string wordsToSearch;
private const string GOOGLE_API_KEY = "AIzaSyD7SHc_jTdUmsYteNArB2f7ME9LXzoTM-g";
private const string GOOGLE_CUSTOM_ENGINE_ID = "002966606582515264909:wuqx1wxqewe";
private const string GOOGLE_SEARCH_URL = "https://www.googleapis.com/customsearch/v1?cx=" +
		GOOGLE_CUSTOM_ENGINE_ID+"&key="+GOOGLE_API_KEY+"&cref&q=";
private const string OXFORD_API_KEY = "63c0e519bf3923479a4f666f1d27e947";
	private const string OXFORD_APP_ID = "d866d9df";
    public AudioSource _audio;
    private const string OXFORD_SEACRH_URL = "https://od-api.oxforddictionaries.com/api/v1/entries/en/{0}/definitions";
	// Use this for initialization
	void Start () {
        wordBg1 = GameObject.Find("wordBg");
        meaningBg1 = GameObject.Find("meaningBg");
        wordBg1.SetActive(false);
        meaningBg1.SetActive(false);
		scanningObject = GameObject.Find ("har-nara");
		scanningObject.SetActive (false);
        _audio = gameObject.GetComponent<AudioSource>();
        buttonObject = GameObject.Find ("scan");
		t1 = GameObject.Find("word");
		t1.SetActive(false);
		t2 = GameObject.Find("meaning");
		t2.SetActive(false);
        status = GameObject.Find("status");
        statusbg = GameObject.Find("statusbg");
        statusbg.SetActive(false);

        
    }
	
	// Update is called once per frame
	void Update () {
		
	}
	public void StartCamera(){
        scanningObject.SetActive(false);
        wordBg1.SetActive(false);
        meaningBg1.SetActive(false);
	StartCoroutine ("TakePhoto");
	}
	public IEnumerator TakePhoto()
	{
		string filePath;
        t1.SetActive(false);
        t2.SetActive(false);
        statusbg.SetActive(false);

        //on mobile platforms persistentDataPath is already prepended to file name when using CaptureScreenshot()
        if (Application.isMobilePlatform) {

				filePath = Application.persistentDataPath + "/image.png";
				ScreenCapture.CaptureScreenshot ("/image.png");
				//must delay here so picture has time to save unfortunatly
				yield return new WaitForSeconds(1.5f);
				//Encode to a PNG
				imageByteArray = File.ReadAllBytes(filePath);
              t1.SetActive(false);
             t2.SetActive(false);

        } else {

				filePath = Application.dataPath + "/StreamingAssets/" + "image.png";
				ScreenCapture.CaptureScreenshot (filePath);
				//must delay here so picture has time to save unfortunatly
				yield return new WaitForSeconds(1.5f);
				//Encode to a PNG
				imageByteArray = File.ReadAllBytes(filePath);
            t1.SetActive(false);
            t2.SetActive(false);
			}

		print ("photo done!!");
        statusbg.SetActive(true);
        status.GetComponent<Text>().text = "photo done!!";
        StartCoroutine("UploadImage");
		buttonObject.SetActive (false);
		//scanningObject.SetActive (true);
        StartCoroutine("MicrosoftSearch");

    }
	public IEnumerator UploadImage(){
        scanningObject.SetActive(true);
        status.GetComponent<Text>().text = "uploading image...";
        print ("uploading image...");
		string url = "https://api.cloudinary.com/v1_1/" + CLOUD_NAME + "/auto/upload/";

		WWWForm myForm = new WWWForm ();
		myForm.AddBinaryData ("file",imageByteArray);
		myForm.AddField ("upload_preset", UPLOAD_PRESET_NAME);

		WWW www = new WWW(url,myForm);
		yield return www;
		print (www.text);
     
        status.GetComponent<Text>().text = "done uploading!!";
        print ("done uploading!");
        scanningObject.SetActive(false);
        //parse resulting string to get image url 
        imageURl = www.text.Split('"', '"')[43];
		print ("IMAGE URL: " + imageURl);
		StartCoroutine ("reverseImageSearch");
	}
	public IEnumerator reverseImageSearch(){
        status.GetComponent<Text>().text = "reverse image search...";
        //create the full search url by adding all 3 together
        scanningObject.SetActive(true);
        string fullSearchURL = BASE_URL + WWW.EscapeURL(imageURl);
		print (fullSearchURL);
WWWForm form = new WWWForm ();
var headers = form.headers;
		headers ["User-Agent"] = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36";
		WWW www = new WWW (fullSearchURL, null, headers);
		//create a new www object and pass in this search url
		yield return www;
		string response = www.text;
		print(response);
		Match m = Regex.Match (response, "style=\"font-style:italic\">(.*?(?=<))");
		wordsToSearch = m.Groups [1].Value;
		print (wordsToSearch);
        status.GetComponent<Text>().text = "word identified!!";
        scanningObject.SetActive(false);
        // StartCoroutine ("GoogleSearchAPI");
        StartCoroutine("SearchWeb");
    }
	public string definition { get; set; }
    public IEnumerator SearchWeb()
    {

        string searchURL = GOOGLE_SEARCH_URL + wordsToSearch;
        WWW www = new WWW(searchURL);
        yield return www;
        string result = www.text;
        print(result);
        string definition = "";
        Regex regex = new Regex("Wikipedia");
        Match match = regex.Match(result);
        if (match.Index != 0)
        {
            regex = new Regex("snippet\": \"(.*?(?=\\.))", RegexOptions.Singleline);
            match = regex.Match(result, match.Index);

            definition = match.Groups[1].Value;
        }
        print(definition);
        if ("".Equals(definition) || definition == null)
        {
            StartCoroutine("OxfordAPI");
        }
        else
        {
            print("google");
            status.GetComponent<Text>().text = "Definition found!!";
            scanningObject.SetActive(false);
            yield return new WaitForSeconds(1.5f);
            
            wordBg1.SetActive(true);
            t1.SetActive(true);
            t1.GetComponent<Text>().text = wordsToSearch + "\n\n";
            if(txt!="")
            {
                t1.GetComponent<Text>().text +="Caption" + "\n\n" + txt;
            }
            //StartCoroutine("MicrosoftSearch");
            //gt.GetComponent<Text>().text = wordsToSearch;

            meaningBg1.SetActive(true);
            t2.SetActive(true);
            
            t2.GetComponent<Text>().text = definition;
            StartCoroutine(play(wordsToSearch));
            
            int len = wordsToSearch.Length;
            yield return new WaitForSeconds(2.0f);
            len += definition.Length;

            StartCoroutine(play(definition));

            yield return new WaitForSeconds(0.1f * len);
            buttonObject.SetActive(true);

        }

    }
    public IEnumerator OxfordAPI(){
        /*string searchURL = GOOGLE_SEARCH_URL + WWW.EscapeURL (wordsToSearch);
        print(searchURL);
            WWW www = new WWW(searchURL);
            yield return www;
            string result = www.text;
            print(result);
            Regex regex = new Regex("Wikipedia");
            Match match = regex.Match(result);
            if (match.Index != 0) {
                regex = new Regex ("snippet\": \"(.*?(?=\\.))", RegexOptions.Singleline);
                match = regex.Match (result, match.Index);
                definition = match.Groups [1].Value;
            }*/
        string words =  wordsToSearch.Replace (" ", "_").ToLower();
        status.GetComponent<Text>().text = "Searching for meaning...";
        string url = String.Format(OXFORD_SEACRH_URL, words);
		var headers = new Dictionary<String, String>();
		headers ["app_id"] = OXFORD_APP_ID;
		headers ["app_key"] = OXFORD_API_KEY;
		headers ["Accept"] = "application/json";
		WWW www = new WWW (url, null, headers);
		yield return(www);
		string result = www.text;
		Match m = Regex.Match(result, "definitions\":.*?(?=\")\"(.*?(?=\"))", RegexOptions.Singleline);
		definition = m.Groups[1].Value;
		print(definition);
       
        status.GetComponent<Text>().text = "Definition found!!";
        yield return new WaitForSeconds(1.5f);
        wordBg1.SetActive(true);
        t1.SetActive(true);
        t1.GetComponent<Text>().text = wordsToSearch + "\n\n";
        if (txt != "")
        {
            t1.GetComponent<Text>().text += "Caption" + "\n\n" + txt;
        }
        StartCoroutine(play(wordsToSearch));
        yield return new WaitForSeconds(2.0f);
        //  if (!t1.GetComponent<Text>().text.Contains(" "))
        meaningBg1.SetActive(true);
        //if (wordsToSearch.Contains(" "))
        //StartCoroutine(DownloadTheAudio(t1.GetComponent<Text>().text));
        t2.SetActive(true);
        t2.GetComponent<Text>().fontSize = 20;
		t2.GetComponent<Text>().text = definition;
        StartCoroutine(play(definition));
        int len = wordsToSearch.Length;
        len += definition.Length;

        yield return new WaitForSeconds(0.1f * len);
        //if (t1.GetComponent<Text>().text.Contains(" "))
        //StartCoroutine(DownloadTheAudio(t1.GetComponent<Text>().text));
        //else
        //StartCoroutine(DownloadTheAudio(t2.GetComponent<Text>().text));
        // scanningObject.SetActive (false);
        buttonObject.SetActive (true);

	}
    IEnumerator play(string word)
    {
        //string url = "http://translate.google.com/translate_tts?tl=" + language + "&q=hello.";

        //string url = "http://translate.google.com/translate_tts?ie=UTF-8&total=1&idx=0&textlen=32&client=tw-ob&q="+word+".&tl=En";

//string url = "http://api.ispeech.org/api/rest?apikey=developerdemokeydeveloperdemokey&action=convert&text="+word+" &voice=usenglishfemale&format=mp3&frequency=44100&bitrate=128&speed=1&startpadding=1&endpadding=1&pitch=110&filename=myaudiofile";
 /* string url = "http://api.voicerss.org/?key=c97675946b0941d6a86a93f44dd46fd9&hl=en-us&src="+word;
         WWW www = new WWW(url);
        yield return www;*/
        Credentials cred = new Credentials("9b54aa5a-221a-46ad-9a20-9a26cb0f34fb","reGOv0w75InZ","https://stream.watsonplatform.net/text-to-speech/api");
        Assistant assist = new Assistant(cred);
        TextToSpeech tts = new TextToSpeech (cred);
        tts.Voice = VoiceType.en_US_Lisa;
        AudioClip au;
        
        tts.ToSpeech(OnSynthesize,OnFail,word);
       // _audio.clip = www.GetAudioClip(false, false, AudioType.MPEG);
      // yield return new WaitForSeconds(0.5f*word.Length);
yield return new WaitForSeconds(0.5f*word.Length);


      //  _audio.Play();
       // yield return new WaitForSeconds(0.2f*word.Length);
    }
    private void OnFail(RESTConnector.Error error, Dictionary<string, object> customData)
{
    Log.Error("ExampleTextToSpeech.OnFail()", "Error received: {0}", error.ToString());
}
   private void OnSynthesize(AudioClip clip, Dictionary<string, object> customData)
{
  PlayClip(clip);
}
private void PlayClip(AudioClip clip)
{
  _audio.clip = clip;
  _audio.Play();
}
    public IEnumerator MicrosoftSearch()
    {

        WWWForm form = new WWWForm();
        form.AddBinaryData("file", imageByteArray);
        var headers = form.headers;
        headers["Ocp-Apim-Subscription-Key"] = MICROSOFT_VISION_KEY;
        WWW www = new WWW(MICROSOFT_VISION_URL, form.data, headers);
        yield return www;
        print(www.text);
        var stuff = JObject.Parse(www.text);

        var desc = stuff["description"];
        var cap = desc["captions"];

        var inter = cap[0];
        txt = (string)inter["text"];
        print(txt);

        //t1.GetComponent<Text>().text += "\n\nCAPTION:\n\n"+txt;



    }
}
