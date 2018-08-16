using IBM.Watson.DeveloperCloud.Connection;
using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Services.Assistant.v1;
using IBM.Watson.DeveloperCloud.Services.TextToSpeech.v1;
using IBM.Watson.DeveloperCloud.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class rectangle : MonoBehaviour {
    public Vector2 _box_start_pos = Vector2.zero;
    public Vector2 _box_end_pos = Vector2.zero;
    public int temph;
    public int tempw;
    public int x = 0;
    public int y = 0;
    public int height = 0;
    public int width = 0;
   public Texture2D SelectionTexture;
    private const string BASE_URL = "http://www.google.com/searchbyimage?hl=ru&image_url=";
    private string imageURl;
    private string wordsToSearch;
    private const string CLOUD_NAME = "dylrioik3";
    //private const string MICROSOFT_VISION_KEY = "6714dabe90b44cc4a60166a1088d7244";
    //private const string MICROSOFT_VISION_URL = "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0/describe?maxCandidates=1";
    private const string UPLOAD_PRESET_NAME = "qylb43yg";
    private const string CLOUDINARY_API_KEY = "284551392584861";
    private const string CLOUDINARY_SIGNATURE = "K5IE-CzFZS5HCZw_DHz5G6MVQVo";
    private const string GOOGLE_API_KEY = "AIzaSyD7SHc_jTdUmsYteNArB2f7ME9LXzoTM-g";
    private const string GOOGLE_CUSTOM_ENGINE_ID = "002966606582515264909:wuqx1wxqewe";
    private const string GOOGLE_SEARCH_URL = "https://www.googleapis.com/customsearch/v1?cx=" +
            GOOGLE_CUSTOM_ENGINE_ID + "&key=" + GOOGLE_API_KEY + "&cref&q=";
    private const string OXFORD_API_KEY = "63c0e519bf3923479a4f666f1d27e947";
    private const string OXFORD_APP_ID = "d866d9df";
    private const string OXFORD_SEACRH_URL = "https://od-api.oxforddictionaries.com/api/v1/entries/en/{0}/definitions";
    public GameObject status;
    public string txt;
    public byte[] bytes;
    public GameObject textobj;
    public GameObject musicobj;
    public UDTEventHandler obj;
    TextMesh tm;
    // Use this for initialization
    void Start () {
        textobj = GameObject.Find("wordobj");
        musicobj = GameObject.Find("music");
        status = GameObject.Find("status");

    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            // Called on the first update where the user has pressed the mouse button.
            if (Input.GetKeyDown(KeyCode.Mouse0))
                _box_start_pos = Input.mousePosition;
            else  // Else we must be in "drag" mode.
                _box_end_pos = Input.mousePosition;
        }
        else
        {
            // Handle the case where the player had been drawing a box but has now released.
            if (_box_end_pos != Vector2.zero && _box_start_pos != Vector2.zero)
            {
                tempw = SelectionTexture.width;
                temph = SelectionTexture.height;
                // StartCoroutine(HandleUnitSelection());
                
                obj.BuildNewTarget();
            }
              //  print("hi");
               // StartCoroutine(HandleUnitSelection());
           // new WaitForSeconds(10);
            // Reset box positions.
            _box_end_pos = _box_start_pos = Vector2.zero;
        }
    }
    public IEnumerator HandleUnitSelection()
    {
        print("hi");
        GameObject button = GameObject.Find("BuildButton");
        button.SetActive(false);
        GameObject Meter = GameObject.Find("QualityMeter");
        Meter.SetActive(false);
        string filePath;
        byte[] imageByteArray;
        if (Application.isMobilePlatform)
        {

            filePath = Application.persistentDataPath + "/image.png";
            ScreenCapture.CaptureScreenshot("/image.png");
            //must delay here so picture has time to save unfortunatly
            yield return new WaitForSeconds(1.5f);
            //Encode to a PNG
            imageByteArray = File.ReadAllBytes(filePath);
            print("**********Photo Done***********");
         //   status.GetComponent<Text>().text += "photo taken\n";

        }
        else
        {

            filePath = Application.dataPath + "/Images/" + "image.png";
            ScreenCapture.CaptureScreenshot(filePath);
            //must delay here so picture has time to save unfortunatly
            yield return new WaitForSeconds(1.5f);
            //Encode to a PNG
            imageByteArray = File.ReadAllBytes(filePath);
            print("**********Photo Done***********");
        //    status.GetComponent<Text>().text += "photo taken\n";
        }
        Texture2D snap = new Texture2D(Screen.height,Screen.width);
        snap.LoadImage(imageByteArray);
        Color[] pix = snap.GetPixels(x, y, width, height);
        Texture2D destTex = new Texture2D(width, height);
        destTex.SetPixels(pix);
        destTex.Apply();
         bytes = destTex.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/Images/" + "image1.png", bytes);
        yield return new WaitForSeconds(1.5f);
        status.GetComponent<Text>().text += "photo taken\n";
    
    button.SetActive(true);
            Meter.SetActive(true);
        StartCoroutine("UploadImage");
    }
    IEnumerator UploadImage()
    {
        print("uploading image...");
        status.GetComponent<Text>().text += "uploading to cloud\n";
        string url = "https://api.cloudinary.com/v1_1/" + CLOUD_NAME + "/auto/upload/";

        WWWForm myForm = new WWWForm();
        myForm.AddBinaryData("file", bytes);
        myForm.AddField("upload_preset", UPLOAD_PRESET_NAME);

        WWW www = new WWW(url, myForm);
        yield return www;
        print(www.text);
        imageURl = www.text.Split('"', '"')[43];
        print("IMAGE URL: " + imageURl);
        status.GetComponent<Text>().text += "uploaded to cloud\n";
        StartCoroutine("reverseImageSearch");
    }
    IEnumerator reverseImageSearch()
    {
        status.GetComponent<Text>().text += "reverse image search\n";
        string fullSearchURL = BASE_URL + WWW.EscapeURL(imageURl);
        print(fullSearchURL);
        WWWForm form = new WWWForm();
        var headers = form.headers;
        headers["User-Agent"] = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36";
        WWW www = new WWW(fullSearchURL, null, headers);
        //create a new www object and pass in this search url
        yield return www;
        string response = www.text;
        print(response);
        Match m = Regex.Match(response, "style=\"font-style:italic\">(.*?(?=<))");
        wordsToSearch = m.Groups[1].Value;
        print(wordsToSearch);
        status.SetActive(false);
        textobj.GetComponent<TextMesh>().text = wordsToSearch;
        textobj.GetComponent<MeshRenderer>().enabled = true;
        //textobj.SetActive(true);
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

            yield return new WaitForSeconds(1.5f);


            if (txt != "")
            {
                print("Caption" + "\n\n" + txt);
            }
            //StartCoroutine("MicrosoftSearch");
            //gt.GetComponent<Text>().text = wordsToSearch;
            //tm.text = wordsToSearch;

            StartCoroutine(play(wordsToSearch));

            int len = wordsToSearch.Length;
            yield return new WaitForSeconds(2.0f);
            len += definition.Length;
            print(definition);
            StartCoroutine(play(definition));

            yield return new WaitForSeconds(0.1f * len);


        }

    }
    public IEnumerator OxfordAPI()
    {
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
        string words = wordsToSearch.Replace(" ", "_").ToLower();
        print("Searching for meaning...");
        string url = String.Format(OXFORD_SEACRH_URL, words);
        var headers = new Dictionary<String, String>();
        headers["app_id"] = OXFORD_APP_ID;
        headers["app_key"] = OXFORD_API_KEY;
        headers["Accept"] = "application/json";
        WWW www = new WWW(url, null, headers);
        yield return (www);
        string result = www.text;
        Match m = Regex.Match(result, "definitions\":.*?(?=\")\"(.*?(?=\"))", RegexOptions.Singleline);
        definition = m.Groups[1].Value;
        print(definition);

        print("Definition found!!");
        yield return new WaitForSeconds(1.5f);

        print(wordsToSearch + "\n\n");
        if (txt != "")
        {
            print("Caption" + "\n\n" + txt);
        }
        StartCoroutine(play(wordsToSearch));
        yield return new WaitForSeconds(2.0f);
        //  if (!t1.GetComponent<Text>().text.Contains(" "))

        //if (wordsToSearch.Contains(" "))
        //StartCoroutine(DownloadTheAudio(t1.GetComponent<Text>().text));

        print(definition);
        StartCoroutine(play(definition));
        int len = wordsToSearch.Length;
        len += definition.Length;

        yield return new WaitForSeconds(0.1f * len);
        //if (t1.GetComponent<Text>().text.Contains(" "))
        //StartCoroutine(DownloadTheAudio(t1.GetComponent<Text>().text));
        //else
        //StartCoroutine(DownloadTheAudio(t2.GetComponent<Text>().text));
        // scanningObject.SetActive (false);


    }
    IEnumerator play(string word)
    {
        //string url = "http://translate.google.com/translate_tts?tl=" + language + "&q=hello.";

        //string url = "http://translate.google.com/translate_tts?ie=UTF-8&total=1&idx=0&textlen=32&client=tw-ob&q="+word+".&tl=En";

        //string url = "http://api.ispeech.org/api/rest?apikey=developerdemokeydeveloperdemokey&action=convert&text="+word+" &voice=usenglishfemale&format=mp3&frequency=44100&bitrate=128&speed=1&startpadding=1&endpadding=1&pitch=110&filename=myaudiofile";
        /* string url = "http://api.voicerss.org/?key=c97675946b0941d6a86a93f44dd46fd9&hl=en-us&src="+word;
                WWW www = new WWW(url);
               yield return www;*/
        Credentials cred = new Credentials("9b54aa5a-221a-46ad-9a20-9a26cb0f34fb", "reGOv0w75InZ", "https://stream.watsonplatform.net/text-to-speech/api");
        Assistant assist = new Assistant(cred);
        TextToSpeech tts = new TextToSpeech(cred);
        tts.Voice = VoiceType.en_US_Lisa;
        AudioClip au;

        tts.ToSpeech(OnSynthesize, OnFail, word);
        // _audio.clip = www.GetAudioClip(false, false, AudioType.MPEG);
        // yield return new WaitForSeconds(0.5f*word.Length);
        yield return new WaitForSeconds(0.5f * word.Length);


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
        musicobj.GetComponent<AudioSource>().clip = clip;
        musicobj.GetComponent<AudioSource>().Play();
    }
    void OnGUI()
    {
        
        // If we are in the middle of a selection draw the texture.
        if (_box_start_pos != Vector2.zero && _box_end_pos != Vector2.zero)
        {
            // Create a rectangle object out of the start and end position while transforming it
            // to the screen's cordinates.
            var rect = new Rect(_box_start_pos.x, Screen.height - _box_start_pos.y,
                                _box_end_pos.x - _box_start_pos.x,
                                -1 * (_box_end_pos.y - _box_start_pos.y));
             x = Mathf.FloorToInt(rect.x);
             y = Mathf.FloorToInt(rect.y);
             width = Mathf.FloorToInt(rect.width);
             height = Mathf.FloorToInt(rect.height);
            // Draw the texture.
            GUI.DrawTexture(rect, SelectionTexture);
        }
    }
}
