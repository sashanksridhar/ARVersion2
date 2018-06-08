/*============================================================================== 
 Copyright (c) 2016-2017 PTC Inc. All Rights Reserved.
 
 Copyright (c) 2015 Qualcomm Connected Experiences, Inc. All Rights Reserved. 
 * ==============================================================================*/
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Vuforia;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using IBM.Watson.DeveloperCloud.Utilities;
using IBM.Watson.DeveloperCloud.Services.Assistant.v1;
using IBM.Watson.DeveloperCloud.Services.TextToSpeech.v1;
using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Connection;
using UnityEngine.UI;

public class UDTEventHandler : MonoBehaviour, IUserDefinedTargetEventHandler
{
    #region PUBLIC_MEMBERS
    /// <summary>
    /// Can be set in the Unity inspector to reference an ImageTargetBehaviour 
    /// that is instantiated for augmentations of new User-Defined Targets.
    /// </summary>
    public ImageTargetBehaviour ImageTargetTemplate;
public GameObject textobj;
public GameObject musicobj;
    public int LastTargetIndex
    {
        get { return (m_TargetCounter - 1) % MAX_TARGETS; }
    }
    #endregion PUBLIC_MEMBERS
public byte[] imageByteArray;
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
		GOOGLE_CUSTOM_ENGINE_ID+"&key="+GOOGLE_API_KEY+"&cref&q=";
private const string OXFORD_API_KEY = "63c0e519bf3923479a4f666f1d27e947";
	private const string OXFORD_APP_ID = "d866d9df";
    private const string OXFORD_SEACRH_URL = "https://od-api.oxforddictionaries.com/api/v1/entries/en/{0}/definitions";
public GameObject status;
public string txt;
TextMesh tm;
    #region PRIVATE_MEMBERS
    const int MAX_TARGETS = 5;
    UserDefinedTargetBuildingBehaviour m_TargetBuildingBehaviour;
    QualityDialog m_QualityDialog;
    ObjectTracker m_ObjectTracker;
    TrackableSettings m_TrackableSettings;
    FrameQualityMeter m_FrameQualityMeter;

    // DataSet that newly defined targets are added to
    DataSet m_UDT_DataSet;

    // Currently observed frame quality
    ImageTargetBuilder.FrameQuality m_FrameQuality = ImageTargetBuilder.FrameQuality.FRAME_QUALITY_NONE;

    // Counter used to name newly created targets
    int m_TargetCounter;
    #endregion //PRIVATE_MEMBERS


    #region MONOBEHAVIOUR_METHODS
    void Start()
    {
        m_TargetBuildingBehaviour = GetComponent<UserDefinedTargetBuildingBehaviour>();

        if (m_TargetBuildingBehaviour)
        {
            m_TargetBuildingBehaviour.RegisterEventHandler(this);
            Debug.Log("Registering User Defined Target event handler.");
        }

        m_FrameQualityMeter = FindObjectOfType<FrameQualityMeter>();
        m_TrackableSettings = FindObjectOfType<TrackableSettings>();
        m_QualityDialog = FindObjectOfType<QualityDialog>();

        if (m_QualityDialog)
        {
            m_QualityDialog.GetComponent<CanvasGroup>().alpha = 0;
        }
        textobj = GameObject.Find("wordobj");
        musicobj = GameObject.Find("music");
        status = GameObject.Find("status");
     //   tm = textobj.GetComponent<TextMesh>();
    //tm.text = "hello";
    //textobj.SetActive(false);
       // print(textobj);
       // print(textobj.GetComponent<TextMesh>().text);
    }
    #endregion //MONOBEHAVIOUR_METHODS


    #region IUserDefinedTargetEventHandler Implementation
    /// <summary>
    /// Called when UserDefinedTargetBuildingBehaviour has been initialized successfully
    /// </summary>
    public void OnInitialized()
    {
        m_ObjectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
        if (m_ObjectTracker != null)
        {
            // Create a new dataset
            m_UDT_DataSet = m_ObjectTracker.CreateDataSet();
            m_ObjectTracker.ActivateDataSet(m_UDT_DataSet);
        }
    }

    /// <summary>
    /// Updates the current frame quality
    /// </summary>
    public void OnFrameQualityChanged(ImageTargetBuilder.FrameQuality frameQuality)
    {
        Debug.Log("Frame quality changed: " + frameQuality.ToString());
        m_FrameQuality = frameQuality;
        if (m_FrameQuality == ImageTargetBuilder.FrameQuality.FRAME_QUALITY_LOW)
        {
            Debug.Log("Low camera image quality");
        }

        m_FrameQualityMeter.SetQuality(frameQuality);
    }

    /// <summary>
    /// Takes a new trackable source and adds it to the dataset
    /// This gets called automatically as soon as you 'BuildNewTarget with UserDefinedTargetBuildingBehaviour
    /// </summary>
    public void OnNewTrackableSource(TrackableSource trackableSource)
    {
        m_TargetCounter++;

        // Deactivates the dataset first
        m_ObjectTracker.DeactivateDataSet(m_UDT_DataSet);

        // Destroy the oldest target if the dataset is full or the dataset 
        // already contains five user-defined targets.
        if (m_UDT_DataSet.HasReachedTrackableLimit() || m_UDT_DataSet.GetTrackables().Count() >= MAX_TARGETS)
        {
            IEnumerable<Trackable> trackables = m_UDT_DataSet.GetTrackables();
            Trackable oldest = null;
            foreach (Trackable trackable in trackables)
            {
                if (oldest == null || trackable.ID < oldest.ID)
                    oldest = trackable;
            }

            if (oldest != null)
            {
                Debug.Log("Destroying oldest trackable in UDT dataset: " + oldest.Name);
                m_UDT_DataSet.Destroy(oldest, true);
            }
        }

        // Get predefined trackable and instantiate it
        ImageTargetBehaviour imageTargetCopy = Instantiate(ImageTargetTemplate);
        imageTargetCopy.gameObject.name = "UserDefinedTarget-" + m_TargetCounter;

        // Add the duplicated trackable to the data set and activate it
        m_UDT_DataSet.CreateTrackable(trackableSource, imageTargetCopy.gameObject);

        // Activate the dataset again
        m_ObjectTracker.ActivateDataSet(m_UDT_DataSet);

        // Extended Tracking with user defined targets only works with the most recently defined target.
        // If tracking is enabled on previous target, it will not work on newly defined target.
        // Don't need to call this if you don't care about extended tracking.
        StopExtendedTracking();
        m_ObjectTracker.Stop();
        m_ObjectTracker.ResetExtendedTracking();
        m_ObjectTracker.Start();

        // Make sure TargetBuildingBehaviour keeps scanning...
        m_TargetBuildingBehaviour.StartScanning();
    }
    #endregion IUserDefinedTargetEventHandler implementation


    #region PUBLIC_METHODS
    /// <summary>
    /// Instantiates a new user-defined target and is also responsible for dispatching callback to 
    /// IUserDefinedTargetEventHandler::OnNewTrackableSource
    /// </summary>
    public void BuildNewTarget()
    {
        /*if (m_FrameQuality == ImageTargetBuilder.FrameQuality.FRAME_QUALITY_MEDIUM ||
            m_FrameQuality == ImageTargetBuilder.FrameQuality.FRAME_QUALITY_HIGH)
        {*/
            // create the name of the next target.
            // the TrackableName of the original, linked ImageTargetBehaviour is extended with a continuous number to ensure unique names
            string targetName = string.Format("{0}-{1}", ImageTargetTemplate.TrackableName, m_TargetCounter);

            // generate a new target:
            m_TargetBuildingBehaviour.BuildNewTarget(targetName, ImageTargetTemplate.GetSize().x);
        /*}
       else
        {
            Debug.Log("Cannot build new target, due to poor camera image quality");
            if (m_QualityDialog)
            {
                StopAllCoroutines();
                m_QualityDialog.GetComponent<CanvasGroup>().alpha = 1;
                StartCoroutine(FadeOutQualityDialog());
            }
        }*/
        
            StartCoroutine("TakePic");
    }
IEnumerator TakePic()
{
string filePath;
GameObject button = GameObject.Find("BuildButton");
button.SetActive(false);
GameObject Meter = GameObject.Find("QualityMeter");
Meter.SetActive(false);
         if (Application.isMobilePlatform) {

				filePath = Application.persistentDataPath + "/image.png";
				ScreenCapture.CaptureScreenshot ("/image.png");
				//must delay here so picture has time to save unfortunatly
				yield return new WaitForSeconds(1.5f);
				//Encode to a PNG
				imageByteArray = File.ReadAllBytes(filePath);
              print("**********Photo Done***********");
              status.GetComponent<Text>().text +="photo taken\n";

        } else {

				filePath = Application.dataPath + "/Images/" + "image.png";
				ScreenCapture.CaptureScreenshot (filePath);
				//must delay here so picture has time to save unfortunatly
				yield return new WaitForSeconds(1.5f);
				//Encode to a PNG
				imageByteArray = File.ReadAllBytes(filePath);
            print("**********Photo Done***********");
            status.GetComponent<Text>().text +="photo taken\n";
			}
            button.SetActive(true);
            Meter.SetActive(true);
             StartCoroutine("UploadImage");
            // StartCoroutine("MicrosoftSearch");
}
 IEnumerator UploadImage(){
 print ("uploading image...");
 status.GetComponent<Text>().text +="uploading to cloud\n";
		string url = "https://api.cloudinary.com/v1_1/" + CLOUD_NAME + "/auto/upload/";

		WWWForm myForm = new WWWForm ();
		myForm.AddBinaryData ("file",imageByteArray);
		myForm.AddField ("upload_preset", UPLOAD_PRESET_NAME);

		WWW www = new WWW(url,myForm);
		yield return www;
		print (www.text);
        imageURl = www.text.Split('"', '"')[43];
		print ("IMAGE URL: " + imageURl);
        status.GetComponent<Text>().text +="uploaded to cloud\n";
        StartCoroutine ("reverseImageSearch");
 }
  IEnumerator reverseImageSearch(){
      status.GetComponent<Text>().text +="reverse image search\n";
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
        status.SetActive(false);
        textobj.GetComponent<TextMesh>().text=wordsToSearch;
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
            
            
            if(txt!="")
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
       print("Searching for meaning...");
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
    /* public IEnumerator MicrosoftSearch()
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
    }*/
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
  musicobj.GetComponent<AudioSource>().clip = clip;
  musicobj.GetComponent<AudioSource>().Play();
}
    #endregion //PUBLIC_METHODS


    #region PRIVATE_METHODS

    IEnumerator FadeOutQualityDialog()
    {
        yield return new WaitForSeconds(1f);
        CanvasGroup canvasGroup = m_QualityDialog.GetComponent<CanvasGroup>();

        for (float f = 1f; f >= 0; f -= 0.1f)
        {
            f = (float)Math.Round(f, 1);
            Debug.Log("FadeOut: " + f);
            canvasGroup.alpha = (float)Math.Round(f, 1);
            yield return null;
        }
    }

    /// <summary>
    /// This method only demonstrates how to handle extended tracking feature when you have multiple targets in the scene
    /// So, this method could be removed otherwise
    /// </summary>
    void StopExtendedTracking()
    {
        // If Extended Tracking is enabled, we first disable it for all the trackables
        // and then enable it only for the newly created target
        bool extTrackingEnabled = m_TrackableSettings && m_TrackableSettings.IsExtendedTrackingEnabled();
        if (extTrackingEnabled)
        {
            StateManager stateManager = TrackerManager.Instance.GetStateManager();

            // 1. Stop extended tracking on all the trackables
            foreach (var tb in stateManager.GetTrackableBehaviours())
            {
                var itb = tb as ImageTargetBehaviour;
                if (itb != null)
                {
                    itb.ImageTarget.StopExtendedTracking();
                }
            }

            // 2. Start Extended Tracking on the most recently added target
            List<TrackableBehaviour> trackableList = stateManager.GetTrackableBehaviours().ToList();
            ImageTargetBehaviour lastItb = trackableList[LastTargetIndex] as ImageTargetBehaviour;
            if (lastItb != null)
            {
                if (lastItb.ImageTarget.StartExtendedTracking())
                    Debug.Log("Extended Tracking successfully enabled for " + lastItb.name);
            }
        }
    }

    #endregion //PRIVATE_METHODS
}