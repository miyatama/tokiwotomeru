using Microsoft.CognitiveServices.SpeechRecognition;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace TokiwotomeruApp
{
    public partial class Form1 : Form
    {

        private VideoCapture video = new VideoCapture(0);
        private Bitmap bitmap;
        // StarPlutinum Stage
        private Mat rgbStapura;
        private Mat alphaStapura;
        private Mat alphaBackground;
        // The World Stage
        private Mat theWorldComplete;
        private Mat theWorldBase;
        private OpenCvSharp.Point centerPosition;
        private int circleSize;
        private int circleMaxSize;
        // start time
        private Mat startTimeComplete;

        private delegate void InvokeDelegate();
        private MicrophoneRecognitionClient micClient;
        private bool isListen = false;
        private enum stage {
            None = 0,
            StarPlutinum = 1,
            TheWorld = 2,
            StartTime = 3
        }
        private stage currentStage = stage.None;

        public Form1()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Mat frame = video.RetrieveMat();
            Mat bgr = frame.Flip(FlipMode.Y);

            switch (currentStage)
            {
                case stage.None:
                    break;
                case stage.StarPlutinum:
                    Mat bgrTemp = new Mat(bgr.Rows, bgr.Cols, MatType.CV_32FC3);
                    bgr.ConvertTo(bgrTemp, MatType.CV_32FC3);
                    if (alphaBackground == null)
                    {
                        // create front end Image and resize
                        Mat stapura = Cv2.ImRead(".\\stapura.png", ImreadModes.Unchanged);
                        Mat resizeStapura = resizeStapura = Mat.Zeros(bgrTemp.Rows, bgrTemp.Cols, MatType.CV_32FC4);
                        Point2f[] src =
                        {
                            new Point2f(0,0),
                            new Point2f(stapura.Cols, 0),
                            new Point2f(stapura.Cols, stapura.Rows),
                            new Point2f(0, stapura.Rows)
                        };
                        /*
                        float startY = bgrTemp.Rows / 3.0f;
                        float startX = bgrTemp.Cols / 3.0f;
                        float height = bgrTemp.Rows - startY;
                        float width = bgrTemp.Cols - startX;
                        */
                        float height = 290;
                        float width = 300;
                        float startY = bgrTemp.Rows - height;
                        float startX = bgrTemp.Cols - width;
                        Point2f[] dst =
                        {
                            new Point2f(startX, startY),
                            new Point2f(startX + width, startY),
                            new Point2f(startX + width, startY + height),
                            new Point2f(startX, startY + height),
                        };
                        Mat resizeMat = Cv2.GetPerspectiveTransform(src, dst);
                        Cv2.WarpPerspective(stapura, resizeStapura, resizeMat, resizeStapura.Size(), InterpolationFlags.Cubic, BorderTypes.Transparent);
                        stapura.Release();
                        stapura = null;
                        resizeMat.Release();
                        resizeMat = null;

                        // split channel 
                        Mat[] chanelSplitedImage = Cv2.Split(resizeStapura);
                        Mat alphaStapura1C = new Mat(bgrTemp.Rows, bgrTemp.Cols, MatType.CV_8UC1);
                        Mat alphaUntiStapura1C = new Mat(bgrTemp.Rows, bgrTemp.Cols, MatType.CV_8UC1);
                        chanelSplitedImage[3].ConvertTo(alphaStapura1C, MatType.CV_8UC1);
                        Cv2.BitwiseNot(alphaStapura1C, alphaUntiStapura1C);
                        Cv2.Normalize(alphaStapura1C, alphaStapura1C, 0.0f, 1.0f, NormTypes.MinMax);
                        Cv2.Normalize(alphaUntiStapura1C, alphaUntiStapura1C, 0.0f, 1.0f, NormTypes.MinMax);

                        Mat rgbStapuraTemp = new Mat(bgrTemp.Rows, bgrTemp.Cols, MatType.CV_32FC3);
                        Mat alphaStapuraTemp = new Mat(bgrTemp.Rows, bgrTemp.Cols, MatType.CV_32FC3);

                        Cv2.Merge(new Mat[]
                            {
                                chanelSplitedImage[0],
                                chanelSplitedImage[1],
                                chanelSplitedImage[2]
                            }
                            , rgbStapuraTemp);
                        Cv2.Merge(new Mat[]
                            {
                                alphaStapura1C,
                                alphaStapura1C,
                                alphaStapura1C
                            }
                            , alphaStapuraTemp);

                        Mat alphaBackgroundTemp = new Mat(bgrTemp.Rows, bgrTemp.Cols, MatType.CV_32FC3);
                        Cv2.Merge(new Mat[]
                            {
                                alphaUntiStapura1C,
                                alphaUntiStapura1C,
                                alphaUntiStapura1C
                            }
                            , alphaBackgroundTemp);

                        rgbStapura = new Mat(bgrTemp.Rows, bgrTemp.Cols, MatType.CV_32FC3);
                        alphaStapura = new Mat(bgrTemp.Rows, bgrTemp.Cols, MatType.CV_32FC3);
                        alphaBackground = new Mat(bgrTemp.Rows, bgrTemp.Cols, MatType.CV_32FC3);
                        rgbStapuraTemp.ConvertTo(rgbStapura, MatType.CV_32FC3);
                        alphaStapuraTemp.ConvertTo(alphaStapura, MatType.CV_32FC3);
                        alphaBackgroundTemp.ConvertTo(alphaBackground, MatType.CV_32FC3);

                        resizeStapura.Release();
                        resizeStapura = null;
                        rgbStapuraTemp.Release();
                        rgbStapuraTemp = null;
                        alphaStapuraTemp.Release();
                        alphaStapuraTemp = null;
                        alphaBackgroundTemp.Release();
                        alphaBackgroundTemp = null;
                        chanelSplitedImage[0].Release();
                        chanelSplitedImage[1].Release();
                        chanelSplitedImage[2].Release();
                        chanelSplitedImage[3].Release();
                        chanelSplitedImage[0] = null; 
                        chanelSplitedImage[1] = null;
                        chanelSplitedImage[2] = null;
                        chanelSplitedImage[3] = null; 
                        chanelSplitedImage = null;
                        alphaStapura1C.Release();
                        alphaStapura1C = null;
                        src = null;
                        dst = null;
                    }
                    {
                        Mat frontend = new Mat(bgrTemp.Rows, bgrTemp.Cols, MatType.CV_32FC3);
                        Mat background = new Mat(bgrTemp.Rows, bgrTemp.Cols, MatType.CV_32FC3);
                        Cv2.Multiply(rgbStapura, alphaStapura, frontend);
                        Cv2.Multiply(bgrTemp, alphaBackground, background);
                        Mat tmp = new Mat(bgrTemp.Rows, bgrTemp.Cols, MatType.CV_32FC3);
                        Cv2.Add(frontend, background, tmp);
                        tmp.ConvertTo(bgr, MatType.CV_8UC3);


                        frontend.Release();
                        frontend = null;
                        background.Release();
                        background = null;
                        tmp.Release();
                        tmp = null;

                        bgrTemp.Release();
                        bgrTemp = null;
                    }
                    break;

                case stage.TheWorld:
                    if (theWorldBase == null)
                    {

                        theWorldBase = new Mat(bgr.Rows, bgr.Cols, MatType.CV_32FC3);
                        Mat theWorldBaseTemp = new Mat(bgr.Rows, bgr.Cols, MatType.CV_32FC3);
                        bgr.ConvertTo(theWorldBaseTemp, MatType.CV_32FC3);
                        Mat frontend = new Mat(bgr.Rows, bgr.Cols, MatType.CV_32FC3);
                        Mat backend = new Mat(bgr.Rows, bgr.Cols, MatType.CV_32FC3);
                        Cv2.Multiply(rgbStapura, alphaStapura, frontend);
                        Cv2.Multiply(theWorldBaseTemp, alphaBackground, backend);
                        Cv2.Add(frontend, backend, theWorldBase);
                        theWorldBaseTemp.Release();
                        theWorldBaseTemp = null;
                        frontend.Release();
                        frontend = null;
                        backend.Release();
                        backend = null;
                        centerPosition = new OpenCvSharp.Point(bgr.Cols / 2.0f, bgr.Rows / 2.0f);
                        circleSize = 1;
                        // magic 5 px
                        circleMaxSize = (int)Math.Ceiling(Math.Sqrt(Math.Pow(centerPosition.X, 2) + Math.Pow(centerPosition.Y, 2))) - 5;

                        theWorldComplete = new Mat(bgr.Rows, bgr.Cols, MatType.CV_32FC3);
                    };
                    if (circleSize < (circleMaxSize))
                    {
                        circleSize += (int)Math.Floor(circleMaxSize / 30.0f);
                        if (circleSize > circleMaxSize)
                        {
                            circleSize = circleMaxSize;
                        }
                        Console.WriteLine(circleSize);

                        // create circle blue mask
                        Mat blueMask = new Mat(bgr.Rows, bgr.Cols, MatType.CV_32FC3);
	                    Cv2.Circle(blueMask, centerPosition, circleSize, new Scalar(1.0d, 0.0d, 0.0d), -1);

                        // create alpha mask
                        Mat[] blueMaskArr = Cv2.Split(blueMask);
                        Mat frontAlphaMask1C = blueMaskArr[0].Clone();

                        Mat blueBackgroundMask = Mat.Ones(bgr.Rows, bgr.Cols, MatType.CV_32FC3);
	                    Cv2.Circle(blueBackgroundMask, centerPosition, circleSize, new Scalar(0.0d, 1.0d, 1.0d), -1);
                        Mat[] backgroundMaskArr = Cv2.Split(blueBackgroundMask);
                        Mat backAlphaMask1C = backgroundMaskArr[0];
                        
                        Cv2.Normalize(frontAlphaMask1C, frontAlphaMask1C, 0.0f, 1.0f, NormTypes.MinMax);
                        Cv2.Normalize(backAlphaMask1C, backAlphaMask1C, 0.0f, 1.0f, NormTypes.MinMax);

                        Mat frontAlphaMask = new Mat(bgr.Rows, bgr.Cols, MatType.CV_32FC3);
                        Mat frontAlphaMaskTemp= new Mat(bgr.Rows, bgr.Cols, MatType.CV_32FC3);
                        Cv2.Merge(new Mat[]
                            {
                                frontAlphaMask1C,
                                frontAlphaMask1C,
                                frontAlphaMask1C
                            }
                            , frontAlphaMaskTemp);
                        frontAlphaMaskTemp.ConvertTo(frontAlphaMask, MatType.CV_32FC3);

                        Mat backAlphaMask = new Mat(bgr.Rows, bgr.Cols, MatType.CV_32FC3);
                        Mat backAlphaMaskTemp = new Mat(bgr.Rows, bgr.Cols, MatType.CV_32FC3);
                        Cv2.Merge(new Mat[]
                            {
                                backAlphaMask1C,
                                backAlphaMask1C,
                                backAlphaMask1C
                            }
                            , backAlphaMaskTemp);
                        backAlphaMaskTemp.ConvertTo(backAlphaMask, MatType.CV_32FC3);

                        // create background
                        Mat background = new Mat(bgr.Rows, bgr.Cols, MatType.CV_32FC3);
                        Cv2.Multiply(theWorldBase, backAlphaMask, background);

                        // create frontend
                        Mat frontend = new Mat(bgr.Rows, bgr.Cols, MatType.CV_32FC3);
                        Cv2.Multiply(theWorldBase, frontAlphaMask, frontend );
                        Cv2.Multiply(frontend, blueMask, frontend );

                        // create Image
                        Cv2.Add(frontend, background, theWorldComplete);
                        theWorldComplete .ConvertTo(bgr, MatType.CV_8UC3);

                        blueMaskArr[0].Release();
                        blueMaskArr[1].Release();
                        blueMaskArr[2].Release();
                        blueBackgroundMask.Release();
                        backgroundMaskArr[0].Release();
                        backgroundMaskArr[1].Release();
                        backgroundMaskArr[2].Release();
                        backgroundMaskArr = null;
                        blueMaskArr = null;
                        frontAlphaMaskTemp.Release();
                        frontAlphaMaskTemp = null;
                        backAlphaMask1C.Release();
                        backAlphaMask1C = null;
                        backAlphaMaskTemp.Release();
                        backAlphaMaskTemp = null;
                        frontAlphaMask1C.Release();
                        frontAlphaMask1C = null;
                        frontAlphaMask.Release();
                        blueMask.Release();
                        backAlphaMask.Release();
                        background.Release();
                        frontend.Release();
                    } else {

                        Mat blueMask = new Mat(bgr.Rows, bgr.Cols, MatType.CV_32FC3, new Scalar(1,0f, 0.0f, 0.0f));
                        Cv2.Multiply(theWorldBase, blueMask, theWorldComplete);
                        theWorldComplete.ConvertTo(bgr, MatType.CV_8UC3);
                        blueMask.Release();
                        blueMask = null;
                    }
                    break;
                case stage.StartTime:
                    if (circleSize >= (circleMaxSize)) { 
                        circleSize = circleMaxSize;
                        startTimeComplete = new Mat(bgr.Rows, bgr.Cols, MatType.CV_32FC3, new Scalar(1,0f, 0.0f, 0.0f));
                    }
                    if (circleSize > 5) {
                        circleSize -= (int)Math.Floor(circleMaxSize / 30.0f);
                        if (circleSize > circleMaxSize)
                        {
                            circleSize = circleMaxSize;
                        }
                        Console.WriteLine(circleSize);

                        // create circle blue mask
                        Mat blueMask = new Mat(bgr.Rows, bgr.Cols, MatType.CV_32FC3);
	                    Cv2.Circle(blueMask, centerPosition, circleSize, new Scalar(1.0d, 0.0d, 0.0d), -1);

                        // create alpha mask
                        Mat[] blueMaskArr = Cv2.Split(blueMask);
                        Mat frontAlphaMask1C = blueMaskArr[0].Clone();

                        Mat blueBackgroundMask = Mat.Ones(bgr.Rows, bgr.Cols, MatType.CV_32FC3);
	                    Cv2.Circle(blueBackgroundMask, centerPosition, circleSize, new Scalar(0.0d, 1.0d, 1.0d), -1);
                        Mat[] backgroundMaskArr = Cv2.Split(blueBackgroundMask);
                        Mat backAlphaMask1C = backgroundMaskArr[0];
                        
                        Cv2.Normalize(frontAlphaMask1C, frontAlphaMask1C, 0.0f, 1.0f, NormTypes.MinMax);
                        Cv2.Normalize(backAlphaMask1C, backAlphaMask1C, 0.0f, 1.0f, NormTypes.MinMax);

                        Mat frontAlphaMask = new Mat(bgr.Rows, bgr.Cols, MatType.CV_32FC3);
                        Mat frontAlphaMaskTemp= new Mat(bgr.Rows, bgr.Cols, MatType.CV_32FC3);
                        Cv2.Merge(new Mat[]
                            {
                                frontAlphaMask1C,
                                frontAlphaMask1C,
                                frontAlphaMask1C
                            }
                            , frontAlphaMaskTemp);
                        frontAlphaMaskTemp.ConvertTo(frontAlphaMask, MatType.CV_32FC3);

                        Mat backAlphaMask = new Mat(bgr.Rows, bgr.Cols, MatType.CV_32FC3);
                        Mat backAlphaMaskTemp = new Mat(bgr.Rows, bgr.Cols, MatType.CV_32FC3);
                        Cv2.Merge(new Mat[]
                            {
                                backAlphaMask1C,
                                backAlphaMask1C,
                                backAlphaMask1C
                            }
                            , backAlphaMaskTemp);
                        backAlphaMaskTemp.ConvertTo(backAlphaMask, MatType.CV_32FC3);

                        // create background
                        Mat background = new Mat(bgr.Rows, bgr.Cols, MatType.CV_32FC3);
                        Cv2.Multiply(theWorldBase, backAlphaMask, background);

                        // create frontend
                        Mat frontend = new Mat(bgr.Rows, bgr.Cols, MatType.CV_32FC3);
                        Cv2.Multiply(theWorldBase, frontAlphaMask, frontend );
                        Cv2.Multiply(frontend, blueMask, frontend );

                        // create Image
                        Cv2.Add(frontend, background, startTimeComplete);
                        startTimeComplete.ConvertTo(bgr, MatType.CV_8UC3);

                        blueMaskArr[0].Release();
                        blueMaskArr[1].Release();
                        blueMaskArr[2].Release();
                        blueBackgroundMask.Release();
                        backgroundMaskArr[0].Release();
                        backgroundMaskArr[1].Release();
                        backgroundMaskArr[2].Release();
                        backgroundMaskArr = null;
                        blueMaskArr = null;
                        frontAlphaMaskTemp.Release();
                        frontAlphaMaskTemp = null;
                        backAlphaMask1C.Release();
                        backAlphaMask1C = null;
                        backAlphaMaskTemp.Release();
                        backAlphaMaskTemp = null;
                        frontAlphaMask1C.Release();
                        frontAlphaMask1C = null;
                        frontAlphaMask.Release();
                        blueMask.Release();
                        backAlphaMask.Release();
                        background.Release();
                        frontend.Release();
                    } else {

                        currentStage = stage.None;
                    }

                    break;

            }
            if(bgr!= null)
            {
                if (bitmap == null)
                {
                    bitmap = bgr.ToBitmap();
                } else {
                    OpenCvSharp.Extensions.BitmapConverter.ToBitmap(bgr, bitmap);
                }
                pictureBox1.Image = bitmap;
            }
            frame.Release();
            frame = null;
            bgr.Release();
            bgr = null;

            if (this.micClient == null) {
                this.CreateMicrophoneRecoClientWithIntent();
                this.CreateMicrophoneRecoClient();
            }
            if (!isListen)
            {
                this.micClient.StartMicAndRecognition();
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            timer1.Start(); 
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

            if(micClient != null)
            {
                micClient.EndMicAndRecognition();
                micClient.Dispose();
                micClient = null;
            }
            timer1.Stop();
        }

        private SpeechRecognitionMode Mode
        {
            get
            {
                return SpeechRecognitionMode.LongDictation;
            }
        }

        private string DefaultLocale
        {
            get { return "ja_JP"; }
        }
        

        public string SubscriptionKey
        {
            get
            {
                return "";
            }
        }


        private string AuthenticationUri
        {
            get
            {
                return "https://api.cognitive.microsoft.com/sts/v1.0/issueToken";
            }
        }

        private string LuisEndpointUrl
        {
            get {
                return "";
            }
        }

        private void CreateMicrophoneRecoClient()
        {
            this.micClient = SpeechRecognitionServiceFactory.CreateMicrophoneClient(
                this.Mode,
                this.DefaultLocale,
                this.SubscriptionKey);
            this.micClient.AuthenticationUri = this.AuthenticationUri;

            // Event handlers for speech recognition results
            this.micClient.OnMicrophoneStatus += this.OnMicrophoneStatus;
            this.micClient.OnPartialResponseReceived += this.OnPartialResponseReceivedHandler;
            if (this.Mode == SpeechRecognitionMode.ShortPhrase)
            {
                this.micClient.OnResponseReceived += this.OnMicShortPhraseResponseReceivedHandler;
            }
            else if (this.Mode == SpeechRecognitionMode.LongDictation)
            {
                this.micClient.OnResponseReceived += this.OnMicDictationResponseReceivedHandler;
            }

            this.micClient.OnConversationError += this.OnConversationErrorHandler;
        }


        private void OnMicDictationResponseReceivedHandler(object sender, SpeechResponseEventArgs e)
        {
            Console.WriteLine("status: " + e.PhraseResponse.RecognitionStatus);
            if (e.PhraseResponse.RecognitionStatus == RecognitionStatus.EndOfDictation ||
                e.PhraseResponse.RecognitionStatus == RecognitionStatus.RecognitionSuccess ||
                e.PhraseResponse.RecognitionStatus == RecognitionStatus.DictationEndSilenceTimeout)
            {
		        if (e.PhraseResponse.Results.Length > 0)
		        {
		            string query = e.PhraseResponse.Results[0].DisplayText;
                    LuisRequest request = new LuisRequest();
                    Task<LuisResult> task = Task.Run(() => request.MakeRequest(query));
                    Task.WaitAll(task);
                    LuisResult result = task.Result;
                    Console.WriteLine("interrupt intent:" + result.topScoringIntent.intent);
                    switch (result.topScoringIntent.intent)
                    {
                        case "sutapura":
                            callStarPlutinum();
                            break;
                        case "zawarudo":
                            if (currentStage == stage.StarPlutinum)
                            {
                                callZawarudo();
                            }
                            break;
                        case "tokiugo":
                            if (currentStage == stage.TheWorld)
                            {
                                callTokiugo();
                            }
                            break;
                    }
                    task.Dispose();
		        }

            }
            isListen = false;
            this.micClient.EndMicAndRecognition();

        }

        private void CreateMicrophoneRecoClientWithIntent()
        {
            this.micClient =
                SpeechRecognitionServiceFactory.CreateMicrophoneClientWithIntentUsingEndpointUrl(
                    this.DefaultLocale,
                    this.SubscriptionKey,
                    this.LuisEndpointUrl);
            this.micClient.AuthenticationUri = this.AuthenticationUri;
            this.micClient.OnIntent += this.OnIntentHandler;

            // Event handlers for speech recognition results
            this.micClient.OnMicrophoneStatus += this.OnMicrophoneStatus;
            this.micClient.OnPartialResponseReceived += this.OnPartialResponseReceivedHandler;
            this.micClient.OnResponseReceived += this.OnMicShortPhraseResponseReceivedHandler;
            this.micClient.OnConversationError += this.OnConversationErrorHandler;
        }

        private void OnMicShortPhraseResponseReceivedHandler(object sender, SpeechResponseEventArgs e)
        {
            BeginInvoke(new InvokeDelegate(() => {
                this.micClient.EndMicAndRecognition();
                Console.WriteLine(e);
            }));
        }

        private void OnMicrophoneStatus(object sender, MicrophoneEventArgs e)
        {
            if (e.Recording) {
                BeginInvoke(new InvokeDelegate(() => {
                    this.Text = "Go Sutapura";
                    Console.WriteLine("start recording");
                    isListen = true;
                }));


            }
        }

        private void OnIntentHandler(object sender, SpeechIntentEventArgs e)
        {
            Console.WriteLine(e.Payload);
        }

        private void OnPartialResponseReceivedHandler(object sender, PartialSpeechResponseEventArgs e)
        {
            Console.WriteLine(e.PartialResult);
        }

        private void OnConversationErrorHandler(object sender, SpeechErrorEventArgs e)
        {
            Console.WriteLine( "Error code: " + e.SpeechErrorCode.ToString() + ", Error text: " + e.SpeechErrorText);
        }

        private void callStarPlutinum()
        {
            currentStage = stage.StarPlutinum;
        }


        private void callZawarudo()
        {
            currentStage = stage.TheWorld;
        }

        private void callTokiugo()
        {
            currentStage = stage.StartTime;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            video.Dispose();
            if (bitmap != null)
            {
                bitmap.Dispose();
                bitmap = null;
            }
            if (rgbStapura != null)
            {
                rgbStapura.Release();
                rgbStapura = null;
            }
            if (rgbStapura != null)
            {
                rgbStapura.Release();
                rgbStapura = null;
            }
            if (alphaStapura != null){
                alphaStapura.Release(); 
                alphaStapura = null;
            }
            if (alphaBackground != null){
                alphaBackground.Release(); 
                alphaBackground = null;
            }
            if (theWorldComplete != null){
                theWorldComplete.Release(); 
                theWorldComplete = null;
            }
            if (theWorldBase != null) {
                theWorldBase.Release(); 
                theWorldBase = null;
            }
            if (startTimeComplete != null){
                startTimeComplete.Release(); 
                startTimeComplete = null;
            }
        }
    }
}
