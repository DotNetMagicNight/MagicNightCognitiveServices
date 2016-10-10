
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Emotion.Contract;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;

namespace MagicNightAzureApplication {
    public class MagicNight {
        private class ImageInfo {
            public readonly Image Image;
            public readonly Face Face;
            public readonly Emotion Emotion;
            public ImageInfo(Image image, Face face, Emotion emotion) {
                Image = image;
                Face = face;
                Emotion = emotion;
            }
        }

        public enum GenderEnum {
            All,
            Male,
            Female
        }

        public enum EmotionEnum {
            All,
            Anger,
            Contempt,
            Disgust,
            Fear,
            Happiness,
            Neutral,
            Sadness,
            Surprise
        }

        private readonly IFaceServiceClient faceServiceClient = new FaceServiceClient("xxx");
        private readonly EmotionServiceClient emotionServiceClient = new EmotionServiceClient("xxx");
        private readonly string[] imageUrls = new[] {
                "https://www.onlyyouforever.com/wp-content/uploads/2015/12/man_sneer_contempt.jpg",
                "http://scalablesocialmedia.com/wp-content/uploads/2014/03/marketing-empathy-contempt.jpg",
                "https://christinehammondcounseling.files.wordpress.com/2012/10/anger_child.jpg",
                "https://zookeepersblog.files.wordpress.com/2012/01/angry-woman-4.jpg",
                "http://tripelio.com/wp-content/uploads/2014/12/gross-face.jpg",
                "http://betanews.com/wp-content/uploads/2014/11/scaredbusinessman.jpg",
                "http://images.mentalfloss.com/sites/default/files/styles/article_640x430/public/istock_000038012212_small.jpg",
                "http://images.clipartpanda.com/happy-man-images-A-Happy-Man.jpg",
                "http://www.healthyblackwoman.com/wp-content/uploads/2013/01/happy-lady.jpg",
                "http://healthtalkwomen.com/wp-content/uploads/2014/05/sad-woman.jpg",
                "https://thumbs.dreamstime.com/x/sad-man-white-background-23197154.jpg",
                "http://southeastidahooralsurgery.com/wp-content/uploads/2012/10/surprised-woman-in-red.jpg",
            };

        private readonly ImageInfo[] _images;
        private readonly PictureBox[] _pictureBoxes;

        public MagicNight(PictureBox[] pictureBoxes, Action doneLoading) {
            _pictureBoxes = pictureBoxes;
            _images = new ImageInfo[imageUrls.Length];
            DownloadAndClassifyImages(doneLoading);
        }

        public void ChangeFilters(GenderEnum gender, EmotionEnum emotion) {

            ///
            ///  Filter Images
            /// 
            var filteredImages = _images.Where(img => {
                GenderEnum currentGender;
                Enum.TryParse(img.Face.FaceAttributes.Gender, true, out currentGender);
                return (gender == GenderEnum.All || currentGender == gender) && (emotion == EmotionEnum.All || (float)typeof(Microsoft.ProjectOxford.Emotion.Contract.Scores).GetProperty(emotion.ToString()).GetValue(img.Emotion.Scores) > 0.1);
            }).Select(img => img.Image).ToArray();

            // Display Images
            DisplayImages(filteredImages);
        }


        // Display up to 3 images in application
        private void DisplayImages(IEnumerable<Image> images) {
            var index = 0;
            foreach(var pictureBox in _pictureBoxes) {
                pictureBox.Image = null;
            }
            foreach(var image in images.Take(_pictureBoxes.Length)) {
                _pictureBoxes[index++].Image = image;
            }
        }

        private async void DownloadAndClassifyImages(Action doneLoading) {
            
            ///
            ///  Classify images via Cognitive Services and cache images locally
            ///  Waiting cursor will display until loading is complete
            /// 
            const string path = "c:\\magicimages\\";
            using(var webClient = new WebClient()) {
                var index = 0;
                foreach(var imageUrl in imageUrls) {
                    var filename = imageUrl.Split('/').Last();
                    Image img = null;
                    var faces = faceServiceClient.DetectAsync(imageUrl, returnFaceAttributes: new[] { FaceAttributeType.Gender });
                    var emotions = emotionServiceClient.RecognizeAsync(imageUrl);
                    try {
                        img = Image.FromFile(path + filename);
                    } catch(Exception) { }
                    if(img == null) {
                        var data = webClient.DownloadData(imageUrl);
                        using(var mem = new MemoryStream(data)) {
                            using(img = Image.FromStream(mem)) {
                                img.Save(path + filename);
                                
                            }
                        }
                    }
                    await Task.WhenAll(new Task[] {
                        faces, emotions
                    });
                    _images[index++] = new ImageInfo(img, faces.Result.FirstOrDefault(), emotions.Result.FirstOrDefault()); 
                }
            }

            // Display normal cursor
            doneLoading();
        }

    }
}
