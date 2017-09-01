namespace MultiDialogsBotMinority
{
    using System;
    using Microsoft.Bot.Builder.FormFlow;
       
    [Serializable]
    public class FaceQuery
    {

        [Prompt("Please Enter your FullName? {||}")]
        public string FullName;

        [Prompt("Please Enter your Age? {||}")]
        public string Age;

        [Prompt("Please Select your Gender {||}")]
        public Gender Gender;

    }

    [Serializable]
  

    public enum Gender
    {
        Male = 1, Female = 2
    };

    


}