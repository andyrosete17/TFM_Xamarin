
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Database;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Widget;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Java.IO;
using LicencePlacte.DTOs;
using LicencePlacte.Enums;
using LicencePlacte.Helper;
using LicensePlateRecognition;
using LicensePlateRecognition.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LicencePlacte
{
    [Activity(Label = "LicencePlate", MainLauncher = true)]
    public class MainActivity : Activity
    {
        /// <summary>
        /// Global variables
        /// </summary>
        ImageView _imageView;
        private LicensePlateDetector _licensePlateDetector;
        private Mat img;
        string imagePath = "";
        static string ocrPath = "/storage/sdcard1";
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

           // _licensePlateDetector = new LicensePlateDetector("x64/");

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            _imageView = FindViewById<ImageView>(Resource.Id.imageView1);
            Button loadPictureBtn = FindViewById<Button>(Resource.Id.LoadPictureBtn);
            loadPictureBtn.Click += LoadPicture;

            Button tesseractBtn = FindViewById<Button>(Resource.Id.TesseractBtn);
            tesseractBtn.Click += ExecuteTesseract;

            if (IsThereAnAppToTakePictures())
            {
                CreateDirectoryForPictures();
                Button takePicture = FindViewById<Button>(Resource.Id.TakePictureBtn);
                takePicture.Click += TakeAPicture;
            }
        }

        private void CreateDirectoryForPictures()
        {
            App._dir = new Java.IO.File(
                Android.OS.Environment.GetExternalStoragePublicDirectory(
                    Android.OS.Environment.DirectoryPictures), "CameraAppDemo");
            if (!App._dir.Exists())
            {
                App._dir.Mkdirs();
            }
        }

        private bool IsThereAnAppToTakePictures()
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            IList<ResolveInfo> availableActivities =
                PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
            return availableActivities != null && availableActivities.Count > 0;
        }

        private void TakeAPicture(object sender, EventArgs eventArgs)
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            App._file = new Java.IO.File(App._dir, string.Format("myPhoto_{0}.jpg", Guid.NewGuid()));
            intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(App._file));
            StartActivityForResult(intent, (int)ActivityEnum.takePicture);
        }

        void LoadPicture(object sender, EventArgs eventArgs)
        {
            Intent = new Intent();
            Intent.SetType("image/*");
            Intent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(Intent.CreateChooser(Intent, "Select Picture"), (int)ActivityEnum.loadPicture);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            switch (requestCode)
            {
                case 0:
                    //Take a picture
                    TakePictureFromCamera();
                    break;
                case 1:
                    //Load from gallery
                    LoadImageFromGallery(requestCode, resultCode, data);
                    break;

                default:
                    break;
            }
        }

        private  void LoadImageFromGallery(int requestCode, Result resultCode, Intent data)
        {
            if ((resultCode == Result.Ok) && (data != null))
            {
                Stream stream = ContentResolver.OpenInputStream(data.Data);
                Bitmap image = BitmapFactory.DecodeStream(stream);

                int height = Resources.DisplayMetrics.HeightPixels;
                int width = _imageView.Height;
                imagePath = GetRealPathFromURI(data.Data);
                Bitmap test = BitmapHelpers.LoadAndResizeBitmap(imagePath,width, height);
                _imageView.SetImageBitmap(test);
            }
        }

        private void TakePictureFromCamera()
        {
            // Make it available in the gallery
            Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
            Android.Net.Uri contentUri = Android.Net.Uri.FromFile(App._file);
            mediaScanIntent.SetData(contentUri);
            SendBroadcast(mediaScanIntent);

            // Display in ImageView. We will resize the bitmap to fit the display.
            // Loading the full sized image will consume to much memory
            // and cause the application to crash.
            int height = Resources.DisplayMetrics.HeightPixels;
            int width = _imageView.Height;
            App.bitmap = App._file.Path.LoadAndResizeBitmap(width, height);
            if (App.bitmap != null)
            {
                _imageView.SetImageBitmap(App.bitmap);
                App.bitmap = null;
            }

            // Dispose of the Java side bitmap.
            GC.Collect();
        }

        private string GetRealPathFromURI(Android.Net.Uri uri)
        {
            string doc_id = "";
            using (var c1 = ContentResolver.Query(uri, null, null, null, null))
            {
                c1.MoveToFirst();
                string document_id = c1.GetString(0);
                doc_id = document_id.Substring(document_id.LastIndexOf(":") + 1);
            }

            string path = null;

            // The projection contains the columns we want to return in our query.
            string selection = MediaStore.Images.Media.InterfaceConsts.Id + " =? ";
            using (var cursor = ContentResolver.Query(MediaStore.Images.Media.ExternalContentUri, null, selection, new string[] { doc_id }, null))
            {
                if (cursor == null)
                    return path;

                var columnIndex = cursor.GetColumnIndexOrThrow(MediaStore.Images.Media.InterfaceConsts.Data);
                cursor.MoveToFirst();
                path = cursor.GetString(columnIndex);
            }
            return path;
        }

        private void ExecuteTesseract(object sender, EventArgs e)
        {
            UMat uImg = new UMat(imagePath, ImreadModes.Color);
            //ProcessImageMethod(uImg, 1);
            //try
            //{
            //    //SetProgressMessage("Checking Tesseract Lang files...");
            //    TesseractDownloadLangFile(ocrPath, "eng");
            //    TesseractDownloadLangFile(ocrPath, "osd");
            //    //SetProgressMessage("Please wait ...");
            //}
            //catch (WebException ex)
            //{
            //    //SetMessage("Unable to download tesseract language file from Internet, please check your Internet connection.");
            //    System.Console.WriteLine(ex);
            //    return;
            //}
            //catch (Exception ex)
            //{
            //    //SetMessage(e.Totring());
            //    System.Console.WriteLine(ex);
            //    return;
            //}


            LicensePlateDetector detector = new LicensePlateDetector(ocrPath + System.IO.Path.DirectorySeparatorChar);


            Stopwatch watch = Stopwatch.StartNew(); // time the detection process

            List<IInputOutputArray> licensePlateImagesList = new List<IInputOutputArray>();
            List<IInputOutputArray> filteredLicensePlateImagesList = new List<IInputOutputArray>();
            List<RotatedRect> licenseBoxList = new List<RotatedRect>();
            List<string> words = detector.DetectLicensePlate(
            uImg,
            licensePlateImagesList,
            filteredLicensePlateImagesList,
            licenseBoxList,1);

            watch.Stop(); //stop the timer

            StringBuilder builder = new StringBuilder();
            builder.Append(string.Format("{0} milli-seconds. ", watch.Elapsed.TotalMilliseconds));
            foreach (string w in words)
                builder.AppendFormat("{0} ", w);
            //SetMessage(builder.ToString());

            foreach (RotatedRect box in licenseBoxList)
            {
                Rectangle rect = box.MinAreaRect();
                CvInvoke.Rectangle(uImg, rect, new Bgr(System.Drawing.Color.Red).MCvScalar, 2);
            }

            SetImageBitmap(uImg.Bitmap);
            uImg.Dispose();
        }

        private void SetImageBitmap(Bitmap image)
        {
            RunOnUiThread(() => { _imageView.SetImageBitmap(image); });
        }

        private void ProcessImageMethod(UMat uImg, int ocr_Method)
        {
            ProcessImage(uImg, ocr_Method);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <param name="ocr_mode"></param>
        /// <param name="count"></param>
        /// <param name="canny_thres">Canny threshold will take 3 values 20, 30, 40, 50</param>
        /// <returns></returns>
        private bool ProcessImage(IInputOutputArray image, int ocr_mode)
        {
            Stopwatch watch = Stopwatch.StartNew(); // time the detection process
            List<IInputOutputArray> licensePlateImagesList = new List<IInputOutputArray>();
            List<IInputOutputArray> filteredLicensePlateImagesList = new List<IInputOutputArray>();
            List<RotatedRect> licenseBoxList = new List<RotatedRect>();
            List<string> words = new List<string>();
            var result = false;
            bool validValue = false;
            UMat filteredPlate = new UMat();
            StringBuilder strBuilder = new StringBuilder();
            CvInvoke.CvtColor(image, filteredPlate, ColorConversion.Bgr2Gray);

            words = _licensePlateDetector.DetectLicensePlate(
                        image,
                        licensePlateImagesList,
                        filteredLicensePlateImagesList,
                        licenseBoxList,
                        ocr_mode);
          

            var validWords = new List<string>();
            var validLicencePlates = new List<IInputOutputArray>();
            for (int w = 0; w < words.Count; w++)
            {
                string replacement2 = Regex.Replace(words[w], @"\t|\n|\r", "");
                string replacement = Regex.Replace(replacement2, "[^0-9a-zA-Z]+", "");
                if (replacement.Length >= 6 && replacement != null)
                {
                    var filteredLicence = FilterLicenceSpain(replacement);
                    if (!string.IsNullOrWhiteSpace(filteredLicence))
                    {
                        validValue = true;
                        if (!validWords.Contains(replacement))
                        {
                            validWords.Add(filteredLicence);
                            validLicencePlates.Add(licensePlateImagesList[w]);
                        }
                    }
                }
            }

            if (validValue)
            {
                ShowResults(image, watch, validLicencePlates, filteredLicensePlateImagesList, licenseBoxList, validWords);
            }
            else
            {
                ShowResults(image, watch, licensePlateImagesList, filteredLicensePlateImagesList, licenseBoxList, words);
            }


            result = true;
            return result;
        }

        /// <summary>
        /// Check if the license has a valid value
        /// </summary>
        /// <param name="replacement"></param>
        /// <returns></returns>
        private string FilterLicenceSpain(string replacement)
        {
            var result = "";
            var mask = new List<string>();
            var charList = replacement.ToCharArray();
            foreach (var character in charList)
            {
                try
                {
                    int.Parse(character.ToString());
                    mask.Add("0");
                }
                catch (Exception)
                {
                    mask.Add("1");
                }
            }

            if (mask.Count >= 8)
            {
                if (string.Join("", mask).Substring(mask.Count - 6) == "100001")
                {
                    replacement = replacement.Substring(replacement.Length - 6);
                    mask = GerenateMak(mask, 6, false);
                }
                else if (string.Join("", mask).Substring(mask.Count - 7) == "1100001"
                        || string.Join("", mask).Substring(mask.Count - 7) == "1000011"
                        || string.Join("", mask).Substring(mask.Count - 7) == "0000111")
                {
                    replacement = replacement.Substring(replacement.Length - 7);
                    mask = GerenateMak(mask, 6, false);
                }
                else if (string.Join("", mask).Substring(mask.Count - 8) == "11000011"
                      || string.Join("", mask).Substring(mask.Count - 8) == "10000011")
                {
                    replacement = replacement.Substring(replacement.Length - 8);
                    mask = GerenateMak(mask, 8, false);
                }
                else if (string.Join("", mask).IndexOf("111") > 0)
                {
                    replacement = replacement.Substring(0, string.Join("", mask).IndexOf("111") + 3);
                    mask = GerenateMak(mask, 7, true);
                }
            }


            switch (mask.Count)
            {
                case 6:
                    {
                        if (string.Join("", mask) == "100001")
                        {
                            if (CheckProvinceEnum(replacement.Substring(0, 1)))
                            {
                                result = replacement;
                            }
                        }
                        break;
                    }
                case 7:
                    {
                        if (string.Join("", mask) == "0000111")
                        {
                            result = replacement;
                        }

                        if (string.Join("", mask) == "1100001")
                        {
                            if (CheckProvinceEnum(replacement.Substring(0, 2)))
                            {
                                result = replacement;
                            }
                        }

                        if (string.Join("", mask) == "1000011")
                        {
                            if (CheckProvinceEnum(replacement.Substring(0, 1)))
                            {
                                result = replacement;
                            }
                        }
                        break;
                    }
                case 8:
                    {
                        if (string.Join("", mask) == "11000011")
                        {
                            if (CheckProvinceEnum(replacement.Substring(0, 2)))
                            {
                                result = replacement;
                            }
                        }
                        else if (string.Join("", mask) == "10000011")
                        {
                            if (CheckProvinceEnum(replacement.Substring(0, 1)))
                            {
                                result = replacement;
                            }
                        }
                        break;
                    }
                default:
                    break;

            }


            return result;
        }

        private static List<string> GerenateMak(List<string> mask, int limit, bool direction)
        {
            var maskTemp = new List<string>();
            if (direction)
            {
                for (int i = 0; i < limit; i++)
                {
                    maskTemp.Add(mask[i]);
                }
            }
            else
            {
                for (int i = mask.Count - limit; i < mask.Count; i++)
                {
                    maskTemp.Add(mask[i]);
                }
            }

            return maskTemp;
        }

        private bool CheckProvinceEnum(string provinceSufix)
        {
            var result = false;
            try
            {
                //string province = Enum.GetName(typeof(ProvincesEnum), provinceSufix);
                ProvincesEnum province = (ProvincesEnum)Enum.Parse(typeof(ProvincesEnum), provinceSufix);

                result = true;
            }
            catch (Exception)
            { }

            return result;
        }

         private void ShowResults(IInputOutputArray image, Stopwatch watch, List<IInputOutputArray> licensePlateImagesList, List<IInputOutputArray> filteredLicensePlateImagesList, List<RotatedRect> licenseBoxList, List<string> words)
        {
            var refinnedWords = new List<string>();
            watch.Stop(); //stop the timer
            TextView textViewTime = new TextView(this);
            textViewTime.Text = string.Format("License Plate Recognition time: {0} milli-seconds", watch.Elapsed.TotalMilliseconds);


            Android.Graphics.Point startPoint = new Android.Graphics.Point(10, 10);
         

            for (int i = 0; i < licensePlateImagesList.Count; i++)
            {
                if (licensePlateImagesList.Count > 0)
                {
                    Mat dest = new Mat();
                    CvInvoke.VConcat(licensePlateImagesList[i], filteredLicensePlateImagesList[i], dest);
                    string replacement2 = Regex.Replace(words[i], @"\t|\n|\r", "");
                    string replacement = Regex.Replace(replacement2, "[^0-9a-zA-Z]+", "");

                    System.Drawing.PointF[] verticesF = licenseBoxList[i].GetVertices();
                    System.Drawing.Point[] vertices = Array.ConvertAll(verticesF, System.Drawing.Point.Round);
                    using (VectorOfPoint pts = new VectorOfPoint(vertices))
                        CvInvoke.Polylines(image, pts, true, new Bgr(System.Drawing.Color.Red).MCvScalar, 2);
                    refinnedWords.Add(replacement);
                }
            }
        }



        private static void TesseractDownloadLangFile(string folder, string lang)
        {
            string subfolderName = "tessdata";
            string folderName = System.IO.Path.Combine(folder, subfolderName);
            ocrPath = folderName;
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }
            string dest = System.IO.Path.Combine(folderName, string.Format("{0}.traineddata", lang));
            if (!System.IO.File.Exists(dest))
                using (WebClient webclient = new WebClient())
                {
                    string source =
                        string.Format("https://github.com/tesseract-ocr/tessdata/blob/4592b8d453889181e01982d22328b5846765eaad/{0}.traineddata?raw=true", lang);

                    System.Console.WriteLine(string.Format("Downloading file from '{0}' to '{1}'", source, dest));
                    using (var client = new WebClient())
                    {
                        client.DownloadFileAsync(new Uri(source), dest);
                    }
                    System.Console.WriteLine(string.Format("Download completed"));
                }
        }
    }

}




