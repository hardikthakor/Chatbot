using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.FormFlow;
using System.Net;
using Newtonsoft.Json;
using MultiDialogsBotMinority;
using CallingApi;
using System.Collections.Generic;
using Microsoft.Bot.Connector;
using System.IO;
using AForge.Video.DirectShow;
using AForge.Video;
using System.Drawing;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using AdaptiveCards;
using Image;
namespace Catagory
{
    public class Rootobject
    {
        public Class1[] Property1 { get; set; }
    }

    public class Class1
    {
        public Facerectangle faceRectangle { get; set; }
        public Scores scores { get; set; }
    }

    public class Facerectangle
    {
        public int height { get; set; }
        public int left { get; set; }
        public int top { get; set; }
        public int width { get; set; }
    }

    public class Scores
    {
        public string anger { get; set; }
        public string contempt { get; set; }
        public string disgust { get; set; }
        public string fear { get; set; }
        public string happiness { get; set; }
        public string neutral { get; set; }
        public string sadness { get; set; }
        public string surprise { get; set; }
    }


    [Serializable]
    public class FaceDialog : IDialog
    {
        public async Task StartAsync(IDialogContext context)
        {
            //String path1 = "C:\\Users\\Godson\\Videos\\s3Images\\image.jpg";
            //if (File.Exists(path1))
            //{
            //    File.Delete(path1);
            //}
            var replyImage = context.MakeMessage();
            replyImage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            replyImage.Attachments = GetImagecardsAttachments();
            await context.PostAsync(replyImage);
            String path = AppDomain.CurrentDomain.BaseDirectory + "\\images\\image.jpg";
            Capture c = new Capture();

            c.cameraCapture(path);


            //c.Upload();

            //var replyImageS3 = context.MakeMessage();
            ////replyImage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            //replyImage.Attachments = ImageCapture();
            //await context.PostAsync(replyImage);
            var replyMessage = context.MakeMessage();
            Attachment attachment = null;
            attachment = ImageCapture();
            replyMessage.Attachments = new List<Attachment> { attachment };
            await context.PostAsync(replyMessage);

            List<Scores> listColleges = await MakeRequest(context);
            var reply = context.MakeMessage();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments = GetApiCardsAttachments(listColleges);
            await context.PostAsync(reply);
           
            
            context.Done<object>(null);

          //  c.Delete();


        }
        public static Attachment ImageCapture()
        {
            String imagePath = AppDomain.CurrentDomain.BaseDirectory + "\\images\\image.jpg";
            //String imagePath = "https://s3-ap-southeast-1.amazonaws.com/emotiondata17/songod.jpg";
            var imageData = Convert.ToBase64String(File.ReadAllBytes(imagePath));
            return new Attachment
            {
                Name = "image.jpg",
                ContentType = "image/jpeg",
                ContentUrl = $"data:image/png;base64,{imageData}"
            };

            }
           

        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            return binaryReader.ReadBytes((int)fileStream.Length);
        }

        private static async Task<List<Scores>> MakeRequest(IDialogContext context)
        {
            var client = new HttpClient();

            // Request headers - replace this example key with your valid key.
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "788195420288455e87bec32b9a7aaab2");

            string uri = "https://westus.api.cognitive.microsoft.com/emotion/v1.0/recognize?";
            HttpResponseMessage response;
            string responseContent;

            String path = AppDomain.CurrentDomain.BaseDirectory + "\\images\\image.jpg";
            // Request body. Try this sample with a locally stored JPEG image.
            byte[] byteData = GetImageAsByteArray(path);

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync(uri, content);
                responseContent = response.Content.ReadAsStringAsync().Result;
            }

            JArray array = JArray.Parse(responseContent);

            //Console.Write("Result:" + array[0]);

            List<Scores> lsc = new List<Scores>();

            Scores sc;

            JObject ScoresJobject;

            foreach (JObject obj in array.Children<JObject>())
            {
                foreach (JProperty singleProp in obj.Properties())
                {
                    string name = singleProp.Name;
                    string value = singleProp.Value.ToString();

                    string data = value;

                    if (name == "scores")
                    {
                        ScoresJobject = JObject.Parse(data);


                        sc = new Scores();

                        sc.anger = (string)ScoresJobject["anger"];
                        sc.contempt = (string)ScoresJobject["contempt"];
                        sc.disgust = (string)ScoresJobject["disgust"];
                        sc.fear = (string)ScoresJobject["fear"];
                        sc.happiness = (string)ScoresJobject["happiness"];
                        sc.neutral = (string)ScoresJobject["neutral"];
                        sc.sadness = (string)ScoresJobject["sadness"];
                        sc.surprise = (string)ScoresJobject["surprise"];

                        lsc.Add(sc);

                    }


                }
            }


            return lsc;


        }
        
        private IList<Attachment> GetApiCardsAttachments(List<Scores> listEmotions)
        {
            List<Attachment> attachment = new List<Attachment>();

            foreach (Scores c in listEmotions)
            {

                var resAttach = GetEmotionCard(
                    c.anger,
                    c.contempt,
                    c.disgust,
                    c.fear,
                    c.happiness,
                    c.neutral,
                    c.sadness,
                    c.surprise
                    );

                attachment.Add(resAttach);


            }

            return attachment;
        }

        private IList<Attachment> GetApiEmotionCardsAttachments(List<Scores> listEmotions)
        {
            List<Attachment> attachment = new List<Attachment>();

            foreach (Scores c in listEmotions)
            {

                var resAttach = GetHeroCard(
                    c.anger,
                    c.happiness
                    );

                attachment.Add(resAttach);


            }

            return attachment;
        }

        private static Attachment GetEmotionCard(string angry, string contempt,  string disgust, string fear,string happy, string neutral,string sad,string surprise)
        {
            AdaptiveCard card = new AdaptiveCard()
            {
                Body = new List<CardElement>()
                {
                    new Container()
                    {
                        Speak = "<s>Hello!</s><s>Are you looking for a flight or a hotel?</s>",
                        Items = new List<CardElement>()
                        {
                            new ColumnSet()
                            {
                                Columns = new List<Column>()
                                {
                                    new Column()
                                    {
                                        Size = ColumnSize.Auto,
                                        Items = new List<CardElement>()
                                        {
                                            new AdaptiveCards.Image()
                                            {
                                                Url = "https://s3-ap-southeast-1.amazonaws.com/botframework21/images/angry.jpg",
                                                Size = ImageSize.Medium,
                                                Style = ImageStyle.Person
                                            }
                                        }
                                    },
                                    new Column()
                                    {
                                        Size = ColumnSize.Stretch,
                                        Items = new List<CardElement>()
                                        {
                                            new TextBlock()
                                            {
                                                Text = "Angry",
                                                Weight = TextWeight.Bolder,
                                                IsSubtle = true
                                            },
                                            new TextBlock()
                                            {

                                                Text = angry,
                                                Wrap = true
                                            }
                                        }
                                    }
                                }
                            },
                            new ColumnSet()
                            {
                                Columns = new List<Column>()
                                {
                                    new Column()
                                    {
                                        Size = ColumnSize.Auto,
                                        Items = new List<CardElement>()
                                        {
                                            new AdaptiveCards.Image()
                                            {
                                                Url = "https://s3-ap-southeast-1.amazonaws.com/botframework21/images/contmp.jpg",
                                                Size = ImageSize.Medium,
                                                Style = ImageStyle.Person
                                            }
                                        }
                                    },
                                    new Column()
                                    {
                                        Size = ColumnSize.Stretch,
                                        Items = new List<CardElement>()
                                        {
                                            new TextBlock()
                                            {
                                                Text =  "Contempt",
                                                Weight = TextWeight.Bolder,
                                                IsSubtle = true
                                            },
                                            new TextBlock()
                                            {
                                                Text = contempt,
                                                Wrap = true
                                            }
                                        }
                                    }
                                }
                            },
                            new ColumnSet()
                            {
                                Columns = new List<Column>()
                                {
                                    new Column()
                                    {
                                        Size = ColumnSize.Auto,
                                        Items = new List<CardElement>()
                                        {
                                            new AdaptiveCards.Image()
                                            {
                                                Url = "https://s3-ap-southeast-1.amazonaws.com/botframework21/images/disgusted-smiley-face.jpg",
                                                Size = ImageSize.Medium,
                                                Style = ImageStyle.Person
                                            }
                                        }
                                    },
                                    new Column()
                                    {
                                        Size = ColumnSize.Stretch,
                                        Items = new List<CardElement>()
                                        {
                                            new TextBlock()
                                            {
                                                Text =  "Disgust",
                                                Weight = TextWeight.Bolder,
                                                IsSubtle = true
                                            },
                                            new TextBlock()
                                            {
                                                Text = disgust,
                                                Wrap = true
                                            }
                                        }
                                    }
                                }
                            },
                            new ColumnSet()
                            {
                                Columns = new List<Column>()
                                {
                                    new Column()
                                    {
                                        Size = ColumnSize.Auto,
                                        Items = new List<CardElement>()
                                        {
                                            new AdaptiveCards.Image()
                                            {
                                                Url = "https://s3-ap-southeast-1.amazonaws.com/botframework21/images/fear.jpg",
                                                Size = ImageSize.Medium,
                                                Style = ImageStyle.Person
                                            }
                                        }
                                    },
                                    new Column()
                                    {
                                        Size = ColumnSize.Stretch,
                                        Items = new List<CardElement>()
                                        {
                                            new TextBlock()
                                            {
                                                Text = "Fear",
                                                Weight = TextWeight.Bolder,
                                                IsSubtle = true
                                            },
                                            new TextBlock()
                                            {

                                                Text = fear,
                                                Wrap = true
                                            }
                                        }
                                    }
                                }
                            },
                            new ColumnSet()
                            {
                                Columns = new List<Column>()
                                {
                                    new Column()
                                    {
                                        Size = ColumnSize.Auto,
                                        Items = new List<CardElement>()
                                        {
                                            new AdaptiveCards.Image()
                                            {
                                                Url = "https://s3-ap-southeast-1.amazonaws.com/botframework21/images/happy.jpg",
                                                Size = ImageSize.Medium,
                                                Style = ImageStyle.Person
                                            }
                                        }
                                    },
                                    new Column()
                                    {
                                        Size = ColumnSize.Stretch,
                                        Items = new List<CardElement>()
                                        {
                                            new TextBlock()
                                            {
                                                Text =  "Happy",
                                                Weight = TextWeight.Bolder,
                                                IsSubtle = true
                                            },
                                            new TextBlock()
                                            {
                                                Text = happy,
                                                Wrap = true
                                            }
                                        }
                                    }
                                }
                            },
                            new ColumnSet()
                            {
                                Columns = new List<Column>()
                                {
                                    new Column()
                                    {
                                        Size = ColumnSize.Auto,
                                        Items = new List<CardElement>()
                                        {
                                            new AdaptiveCards.Image()
                                            {
                                                Url = "https://s3-ap-southeast-1.amazonaws.com/botframework21/images/neutral.jpg",
                                                Size = ImageSize.Medium,
                                                Style = ImageStyle.Person
                                            }
                                        }
                                    },
                                    new Column()
                                    {
                                        Size = ColumnSize.Stretch,
                                        Items = new List<CardElement>()
                                        {
                                            new TextBlock()
                                            {
                                                Text =  "Neutral",
                                                Weight = TextWeight.Bolder,
                                                IsSubtle = true
                                            },
                                            new TextBlock()
                                            {
                                                Text = neutral,
                                                Wrap = true
                                            }
                                        }
                                    }
                                }
                            },
                            new ColumnSet()
                            {
                                Columns = new List<Column>()
                                {
                                    new Column()
                                    {
                                        Size = ColumnSize.Auto,
                                        Items = new List<CardElement>()
                                        {
                                            new AdaptiveCards.Image()
                                            {
                                                Url = "https://s3-ap-southeast-1.amazonaws.com/botframework21/images/sad.jpg",
                                                Size = ImageSize.Medium,
                                                Style = ImageStyle.Person
                                            }
                                        }
                                    },
                                    new Column()
                                    {
                                        Size = ColumnSize.Stretch,
                                        Items = new List<CardElement>()
                                        {
                                            new TextBlock()
                                            {
                                                Text = "Sad",
                                                Weight = TextWeight.Bolder,
                                                IsSubtle = true
                                            },
                                            new TextBlock()
                                            {

                                                Text = sad,
                                                Wrap = true
                                            }
                                        }
                                    }
                                }
                            },
                            new ColumnSet()
                            {
                                Columns = new List<Column>()
                                {
                                    new Column()
                                    {
                                        Size = ColumnSize.Auto,
                                        Items = new List<CardElement>()
                                        {
                                            new AdaptiveCards.Image()
                                            {
                                                Url = "https://s3-ap-southeast-1.amazonaws.com/botframework21/images/surprised.jpg",
                                                Size = ImageSize.Medium,
                                                Style = ImageStyle.Person
                                            }
                                        }
                                    },
                                    new Column()
                                    {
                                        Size = ColumnSize.Stretch,
                                        Items = new List<CardElement>()
                                        {
                                            new TextBlock()
                                            {
                                                Text =  "Surprise",
                                                Weight = TextWeight.Bolder,
                                                IsSubtle = true
                                            },
                                            new TextBlock()
                                            {
                                                Text = surprise,
                                                Wrap = true
                                            }
                                        }
                                    }
                                }
                            },


                        }
                    }
                },


            };


            Attachment attachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };

            return attachment;
        }


        private void ShowOptionsAsync(IDialogContext context, List<Scores> lst )
        {
             //fs = lst.ElementAt(0).ToString;

                AdaptiveCard card = new AdaptiveCard()
                {
                    Body = new List<CardElement>()
                {
                    new Container()
                    {
                        Speak = "<s>Hello!</s><s>Are you looking for a flight or a hotel?</s>",
                        Items = new List<CardElement>()
                        {
                            new ColumnSet()
                            {
                                Columns = new List<Column>()
                                {
                                    new Column()
                                    {
                                        Size = ColumnSize.Auto,
                                        Items = new List<CardElement>()
                                        {
                                            new AdaptiveCards.Image()
                                            {
                                                Url = "C:\\Users\\Godson\\Documents\\visual studio 2017\\Projects\\EmotionBot\\EmotionBot\\images\\angry.jpg",
                                                Size = ImageSize.Medium,
                                                Style = ImageStyle.Person
                                            }
                                        }
                                    },
                                    new Column()
                                    {
                                        Size = ColumnSize.Stretch,
                                        Items = new List<CardElement>()
                                        {
                                            new TextBlock()
                                            {
                                                Text = "Happy",
                                                Weight = TextWeight.Bolder,
                                                IsSubtle = true
                                            },
                                            new TextBlock()
                                            {

                                                Text = "Happy",
                                                Wrap = true
                                            }
                                        }
                                    }
                                }
                            }
                            //new ColumnSet()
                            //{
                            //    Columns = new List<Column>()
                            //    {
                            //        new Column()
                            //        {
                            //            Size = ColumnSize.Auto,
                            //            Items = new List<CardElement>()
                            //            {
                            //                new AdaptiveCards.Image()
                            //                {
                            //                    Url = "C:\\Users\\Godson\\Documents\\visual studio 2017\\Projects\\EmotionBot\\EmotionBot\\images\\angry.jpg",
                            //                    Size = ImageSize.Medium,
                            //                    Style = ImageStyle.Person
                            //                }
                            //            }
                            //        },
                            //        new Column()
                            //        {
                            //            Size = ColumnSize.Stretch,
                            //            Items = new List<CardElement>()
                            //            {
                            //                new TextBlock()
                            //                {
                            //                    Text =  "Sad",
                            //                    Weight = TextWeight.Bolder,
                            //                    IsSubtle = true
                            //                },
                            //                new TextBlock()
                            //                {
                            //                    Text = scores.anger,
                            //                    Wrap = true
                            //                }
                            //            }
                            //        }
                            //    }
                            //},
                            //new ColumnSet()
                            //{
                            //    Columns = new List<Column>()
                            //    {
                            //        new Column()
                            //        {
                            //            Size = ColumnSize.Auto,
                            //            Items = new List<CardElement>()
                            //            {
                            //                new AdaptiveCards.Image()
                            //                {
                            //                    Url = "C:\\Users\\Godson\\Documents\\visual studio 2017\\Projects\\EmotionBot\\EmotionBot\\images\\angry.jpg",
                            //                    Size = ImageSize.Medium,
                            //                    Style = ImageStyle.Person
                            //                }
                            //            }
                            //        },
                            //        new Column()
                            //        {
                            //            Size = ColumnSize.Stretch,
                            //            Items = new List<CardElement>()
                            //            {
                            //                new TextBlock()
                            //                {
                            //                    Text =  "Contmp",
                            //                    Weight = TextWeight.Bolder,
                            //                    IsSubtle = true
                            //                },
                            //                new TextBlock()
                            //                {
                            //                    Text = scores.anger,
                            //                    Wrap = true
                            //                }
                            //            }
                            //        }
                            //    }
                            //},
                            //new ColumnSet()
                            //{
                            //    Columns = new List<Column>()
                            //    {
                            //        new Column()
                            //        {
                            //            Size = ColumnSize.Auto,
                            //            Items = new List<CardElement>()
                            //            {
                            //                new AdaptiveCards.Image()
                            //                {
                            //                    Url = "C:\\Users\\Godson\\Documents\\visual studio 2017\\Projects\\EmotionBot\\EmotionBot\\images\\angry.jpg",
                            //                    Size = ImageSize.Medium,
                            //                    Style = ImageStyle.Person
                            //                }
                            //            }
                            //        },
                            //        new Column()
                            //        {
                            //            Size = ColumnSize.Stretch,
                            //            Items = new List<CardElement>()
                            //            {
                            //                new TextBlock()
                            //                {
                            //                    Text =  "Neutral",
                            //                    Weight = TextWeight.Bolder,
                            //                    IsSubtle = true
                            //                },
                            //                new TextBlock()
                            //                {
                            //                    Text = scores.contempt,
                            //                    Wrap = true
                            //                }
                            //            }
                            //        }
                            //    }
                            //},
                            //new ColumnSet()
                            //{
                            //    Columns = new List<Column>()
                            //    {
                            //        new Column()
                            //        {
                            //            Size = ColumnSize.Auto,
                            //            Items = new List<CardElement>()
                            //            {
                            //                new AdaptiveCards.Image()
                            //                {
                            //                    Url = "C:\\Users\\Godson\\Documents\\visual studio 2017\\Projects\\EmotionBot\\EmotionBot\\images\\angry.jpg",
                            //                    Size = ImageSize.Medium,
                            //                    Style = ImageStyle.Person
                            //                }
                            //            }
                            //        },
                            //        new Column()
                            //        {
                            //            Size = ColumnSize.Stretch,
                            //            Items = new List<CardElement>()
                            //            {
                            //                new TextBlock()
                            //                {
                            //                    Text =  "Fear",
                            //                    Weight = TextWeight.Bolder,
                            //                    IsSubtle = true
                            //                },
                            //                new TextBlock()
                            //                {
                            //                    Text = scores.happiness,
                            //                    Wrap = true
                            //                }
                            //            }
                            //        }
                            //    }
                            //},
                            //new ColumnSet()
                            //{
                            //    Columns = new List<Column>()
                            //    {
                            //        new Column()
                            //        {
                            //            Size = ColumnSize.Auto,
                            //            Items = new List<CardElement>()
                            //            {
                            //                new AdaptiveCards.Image()
                            //                {
                            //                    Url ="C:\\Users\\Godson\\Documents\\visual studio 2017\\Projects\\EmotionBot\\EmotionBot\\images\\angry.jpg",
                            //                    Size = ImageSize.Medium,
                            //                    Style = ImageStyle.Person
                            //                }
                            //            }
                            //        },
                            //        new Column()
                            //        {
                            //            Size = ColumnSize.Stretch,
                            //            Items = new List<CardElement>()
                            //            {
                            //                new TextBlock()
                            //                {
                            //                    Text =  "Surprised",
                            //                    Weight = TextWeight.Bolder,
                            //                    IsSubtle = true
                            //                },
                            //                new TextBlock()
                            //                {
                            //                    Text =scores.disgust,
                            //                    Wrap = true
                            //                }
                            //            }
                            //        }
                            //    }
                            //},
                            //new ColumnSet()
                            //{
                            //    Columns = new List<Column>()
                            //    {
                            //        new Column()
                            //        {
                            //            Size = ColumnSize.Auto,
                            //            Items = new List<CardElement>()
                            //            {
                            //                new AdaptiveCards.Image()
                            //                {
                            //                    Url ="C:\\Users\\Godson\\Documents\\visual studio 2017\\Projects\\EmotionBot\\EmotionBot\\images\\angry.jpg",
                            //                    Size = ImageSize.Medium,
                            //                    Style = ImageStyle.Person
                            //                }
                            //            }
                            //        },
                            //        new Column()
                            //        {
                            //            Size = ColumnSize.Stretch,
                            //            Items = new List<CardElement>()
                            //            {
                            //                new TextBlock()
                            //                {
                            //                    Text =  "Disgused",
                            //                    Weight = TextWeight.Bolder,
                            //                    IsSubtle = true
                            //                },
                            //                new TextBlock()
                            //                {
                            //                    Text = "digust",
                            //                    Wrap = true
                            //                }
                            //            }
                            //        }
                            //    }
                            //},
                            //new ColumnSet()
                            //{
                            //    Columns = new List<Column>()
                            //    {
                            //        new Column()
                            //        {
                            //            Size = ColumnSize.Auto,
                            //            Items = new List<CardElement>()
                            //            {
                            //                new AdaptiveCards.Image()
                            //                {
                            //                    Url ="C:\\Users\\Godson\\Documents\\visual studio 2017\\Projects\\EmotionBot\\EmotionBot\\images\\angry.jpg",
                            //                    Size = ImageSize.Medium,
                            //                    Style = ImageStyle.Person
                            //                }
                            //            }
                            //        },
                            //        new Column()
                            //        {
                            //            Size = ColumnSize.Stretch,
                            //            Items = new List<CardElement>()
                            //            {
                            //                new TextBlock()
                            //                {
                            //                    Text =  "Angry",
                            //                    Weight = TextWeight.Bolder,
                            //                    IsSubtle = true
                            //                },
                            //                new TextBlock()
                            //                {
                            //                    Text = "anger",
                            //                    Wrap = true
                            //                }
                            //            }
                            //        }
                            //    }
                            //}
                        }
                    }
                },


                };
            
            Attachment attachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };

            var reply = context.MakeMessage();
            reply.Attachments.Add(attachment);

            //await context.PostAsync(reply, CancellationToken.None);



        }


        //private IForm<FaceQuery> BuildFaceForm()
        //{
        //    OnCompletionAsyncDelegate<FaceQuery> processHotelsSearch = async (context, state) =>
        //    {
        //        //  String msg = "Hello " + state.FullName + ", for Category " + state.Category + ", Admission type " + state.AdmissionType + ", and Gender " + state.Gender + " with MHCET Marks " + state.Marks + ". The Recommended Collges are ....";

        //        List<EmotionCard> listColleges = apiCall(state.FullName, state.Age, state.Gender);

        //        var reply = context.MakeMessage();

        //        reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
        //        reply.Attachments = GetApiCardsAttachments(listColleges);

        //        // reply.Text = "";

        //        // await context.PostAsync(msg);
        //        await context.PostAsync(reply);

        //    };

        //    return new FormBuilder<FaceQuery>()
        //        .AddRemainingFields()
        //        .OnCompletion(processHotelsSearch)
        //        .Build();


        //}



        private List<EmotionCard> apiCall(string name, string age, Gender gender)
        {
            var myName = name;
            var myAge = age;
            var myGender = gender.ToString();

            // Write the Emotion Calling part here 

            FaceDialog sd = new FaceDialog();

            // Capturing the Photo 
            // sd.Capture();

            String imagePath = "C:\\Users\\Dell\\Pictures\\image.jpg";

            var imageData = Convert.ToBase64String(File.ReadAllBytes(imagePath));



            String emotion = MakeRequestAsync(imagePath).Result;

            
            // String result = await MakeRequest(imagePath);


            // dynamic parsedArray = JsonConvert.DeserializeObject(emotion);
            dynamic parsedArray2 = JsonConvert.DeserializeObject(emotion);

            EmotionCard ec;
            List<EmotionCard> clg = new List<EmotionCard>();

            foreach (dynamic item in parsedArray2)
            {
                ec = new EmotionCard();
                ec.angry = item.collegeName;
                ec.sad = item.description;
                ec.happy = item.imgUrl;

                clg.Add(ec);

            }

            return clg;

        }

        public static byte[] GetEmotionImageAsByteArray(Stream imageFilePath)
        {
           // FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);

            MemoryStream stream = new MemoryStream();
            BinaryReader binaryReader = new BinaryReader(imageFilePath);
            return binaryReader.ReadBytes((int)stream.Length);
        }


        public async Task<string> MakeRequestAsync(String imageFilePath)
        {

            var client = new HttpClient();

            Capture c = new Capture();

            // Request headers - replace this example key with your valid key.
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "788195420288455e87bec32b9a7aaab2");

            // NOTE: You must use the same region in your REST call as you used to obtain your subscription keys.
            //   For example, if you obtained your subscription keys from westcentralus, replace "westus" in the
            //   URI below with "westcentralus".
            string uri = "https://westus.api.cognitive.microsoft.com/emotion/v1.0/recognize?";
            HttpResponseMessage response;
            string responseContent;

            // Request body. Try this sample with a locally stored JPEG image.
             byte[] byteData = GetImageAsByteArray(imageFilePath);

           // Stream awsFileStream = c.Download();

            //byte[] byteData = GetImageAsByteArray(byteData);

            using (var content = new ByteArrayContent(byteData))
            {

                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                //response = await client.PostAsync(uri, content);
                response = client.PostAsync(uri, content).Result;
                responseContent = response.Content.ReadAsStringAsync().Result;
            }



            // var data = JsonConvert.DeserializeObject(responseContent);

            // Newtonsoft.Json.Linq.JObject studentObj = JObject.Parse(responseContent);

            String responseData = JsonConvert.SerializeObject(responseContent);
            // Console.WriteLine(responseData);
            return responseContent;
        }


        private static Attachment GetHeroCard(string title, string subtitle)
        {
            var heroCard = new HeroCard
            {
                Title = title,
                Subtitle = subtitle,
             
                
            };

            return heroCard.ToAttachment();
        }

        private async Task ResumeAfterHotelsFormDialog(IDialogContext context, IAwaitable<FaceQuery> result)
        {
            try
            {
                var searchQuery = await result;

                //  var hotels = await this.GetHotelsAsync(searchQuery);

                //  await context.PostAsync($"I found in total {hotels.Count()} hotels for your dates:");

                //  var resultMessage = context.MakeMessage();
                //  resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                // resultMessage.Attachments = new List<Attachment>();

                //var reply = context.MakeMessage();

                //reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                //reply.Attachments = GetCardsAttachments();

                //await context.PostAsync(reply);



                // context.Wait(this.MessageReceivedAsync);

                //foreach (var hotel in hotels)
                //{
                //    HeroCard heroCard = new HeroCard()
                //    {
                //        Title = hotel.Name,
                //        Subtitle = $"{hotel.Rating} starts. {hotel.NumberOfReviews} reviews. From ${hotel.PriceStarting} per night.",
                //        Images = new List<CardImage>()
                //        {
                //            new CardImage() { Url = hotel.Image }
                //        },
                //        Buttons = new List<CardAction>()
                //        {
                //            new CardAction()
                //            {
                //                Title = "More details",
                //                Type = ActionTypes.OpenUrl,
                //                Value = $"https://www.bing.com/search?q=hotels+in+" + HttpUtility.UrlEncode(hotel.Location)
                //            }
                //        }
                //    };

                //    resultMessage.Attachments.Add(heroCard.ToAttachment());
                //}

                //  await context.PostAsync(resultMessage);
            }
            catch (FormCanceledException ex)
            {
                string reply;

                if (ex.InnerException == null)
                {
                    reply = "You have canceled the operation. Quitting from the HotelsDialog";
                }
                else
                {
                    reply = $"Oops! Something went wrong :( Technical Details: {ex.InnerException.Message}";
                }

                await context.PostAsync(reply);
            }
            finally
            {
                context.Done<object>(null);
            }
        }


        //private static IList<Attachment> ImageFromS3()
        //{
        //    String imagePath = AppDomain.CurrentDomain.BaseDirectory + "images//images.jpg";
        //    return new List<Attachment>()
        //    {
        //          GetImagecard(
        //            "Your Image",
        //             new CardImage(url: imagePath))
        //             //new CardImage(url: "https://s3-ap-southeast-1.amazonaws.com/emotiondata17/songod.jpg"))

        //    };
        //}

        private static IList<Attachment> GetImagecardsAttachments()
        {
            return new List<Attachment>()
            {
                  GetImagecard(
                    "Welcome To RockStar Emotions BOT!!!",
                     new CardImage(url: "https://s3-us-west-2.amazonaws.com/awsazurebot/emotionsBot/rockstar_drib.gif"))

            };
        }


      

        private static Attachment GetImagecard(string title, CardImage cardImage)
        {
            var imagecard = new HeroCard
            {
                Title = title,
                Images = new List<CardImage>() { cardImage },

            };

            return imagecard.ToAttachment();
        }

    }


}