using System.ComponentModel.DataAnnotations;

namespace OutOut.Constants.Enums
{
    public enum IssueTypes
    {
        Unspecified,
        [Display(Name = "Listing contains bad language")]
        ListingContainsBadLanguage,
        [Display(Name = "Listing contains nudity")]
        ListingContainsNudity,
        [Display(Name = "Listing contains racism or other prejudices")]
        ListingContainsRacismOrOtherPrejudices,
        [Display(Name = "Listing contains copyright trademarks")]
        ListingContainsCopyrightTrademarks,
        [Display(Name = "Listing does not meet the Terms and Conditions")]
        ListingDoesNotMeetTheTermsAndConditions,
        [Display(Name = "Listing is against UAE Laws")]
        ListingIsAgainstUAELaws,
        [Display(Name = "Listing is abusive or harmful")]
        ListingIsAbusiveOrHarmful,
        [Display(Name = "Listing is defamatory")]
        ListingIsDefamatory,
        [Display(Name = "Listing promotes hate speech")]
        ListingPromotesHateSpeech,
        [Display(Name = "Listing is listed more than once")]
        ListingIsListedMoreThanOnce,
        [Display(Name = "Listing is not real or expired")]
        ListingIsNotRealOrExpired,
        [Display(Name = "Listing is of a political agenda")]
        ListingIsOfAPoliticalAgenda,
        [Display(Name = "The image is inappropriate")]
        TheImageIsInappropriate,
        [Display(Name = "The image quality is poor")]
        TheImageQualityIsPoor,
        [Display(Name = "The information is incorrect")]
        TheInformationIsIncorrect,
        [Display(Name = "The App is not working")]
        TheAppIsNotWorking,
        [Display(Name = "Can I request a refund")]
        CanIRequestARefund,
        [Display(Name = "My offers are not working")]
        MyOffersAreNotWorking,
        [Display(Name = "My app is not updating")]
        MyAppIsNotUpdating,
        [Display(Name = "Help I have a payment issue")]
        HelpIHaveAPaymentIssue,
        [Display(Name = "Just want to say hello")]
        JustWantToSayHello
    }
}
