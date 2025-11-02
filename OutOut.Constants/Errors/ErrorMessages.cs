namespace OutOut.Constants.Errors
{
    public static class ErrorMessages
    {
        public static string ToMessage(this ErrorCodes code)
        {
            return code switch
            {
                #region General
                ErrorCodes.ValidationErrors => "One or more validation errors occurred.",

                ErrorCodes.YouVeReachedLastPage => "you have reached the last page.",
                ErrorCodes.PageNotFound => "The requested page could not be found.",
                ErrorCodes.PageSizeCannotBeZero => "Page Size Cannot Be Zero.",

                ErrorCodes.InvalidNullParameters => "Invalid request, null parameters where not accepted.",
                ErrorCodes.RequestNotFound => "Request was not found.",
                ErrorCodes.InvalidUser => "The booking is not made with your account.",
                ErrorCodes.CityAlreadyExists => "City name already exists, try again with a different name.",
                ErrorCodes.AreaAlreadyExists => "Area already exists in this city, try again with a different area.",
                ErrorCodes.NoChangesHaveBeenMade => "Unable to save because no changes have been made.",
                ErrorCodes.RequestForApprovalNotFound => "No pending request is found.",
                #endregion

                #region Users / User Management
                ErrorCodes.IdentityErrors => "One or more identity errors occured.",

                ErrorCodes.YouDontHaveAccessToThisVenue => "You don't have access to this venue.",
                ErrorCodes.YouDontHaveAccessToThisEvent => "You don't have access to this event.",

                ErrorCodes.Unauthorized => "You are not authorized for this action.",
                ErrorCodes.ProfileNotFound => "Profile not found",
                ErrorCodes.InvalidLogin => "You entered a wrong email or password!",
                ErrorCodes.InvalidRefreshToken => "Invalid Refresh Token.",
                ErrorCodes.InvalidToken => "Invalid Access Token.",
                ErrorCodes.IdentityErrors_FailedCustomerRegistration => "One or more issues occured while registering you account.",
                ErrorCodes.DuplicateEmail => "This email is already registered.",
                ErrorCodes.EmailNotFound => "The email you provided does not exist.",
                ErrorCodes.UserNotFound => "This user was not found.",
                ErrorCodes.EmailAlreadyConfirmed => "This email was already confirmed.",
                ErrorCodes.UnverifiedEmail => "This email was not verified.",

                ErrorCodes.InvalidOTP => "Please make sure that the code entered is correct.",
                ErrorCodes.OTPExpired => "The code you entered has expired you can choose to resend the code again.",
                ErrorCodes.OTPNotGenerated => "Error, generate an OTP first.",
                ErrorCodes.VerificationRequestsLimitReached => "Unable to send verification code as you can only receive one code per minute or 10 codes per hour.",
                ErrorCodes.ResetOTPRequestsLimitReached => "Unable to send password verification code as you can only receive one code per minute or 10 codes per hour.",
                ErrorCodes.WrongOldPassword => "Your old password is not correct.",
                ErrorCodes.CouldntUpdateProfileImage => "An error occured while trying to update profile image.",

                ErrorCodes.UnsupportedCountry => "Outout is not yet available in this location.",
                #endregion

                #region Offers
                ErrorCodes.OfferNotFound => "Offer was not found.",
                ErrorCodes.ExceededMaxUsagesPerDay => "You've exceeded the maximum number of usages per day, try again tomorrow.",
                ErrorCodes.ExceededMaxUsagesPerYear => "You've exceeded the maximum number of usages per year, try again later.",
                ErrorCodes.OfferCannotBeRedeemedRightNow => "This offer cannot be redeemed right now, try again in another time.",
                ErrorCodes.OfferIsNotActive => "Invalid Action, offer is inactive.",
                ErrorCodes.InvalidPINCodeForOffer => "Invalid PIN code for redeeming this offer.",
                ErrorCodes.OfferHasExpired => "This offer has expired.",
                #endregion

                #region Venue
                ErrorCodes.VenueNotFound => "Venue was not found.",
                ErrorCodes.UnavailableVenue => "Venue is currently unavailable.",
                ErrorCodes.OpenDaysCantOverlap => "Opening days can't overlap or chosen twice.",
                ErrorCodes.VenueHasAssignedLoyalty => " Venue already has an assigned loyalty.",
                ErrorCodes.OpenTimesIsRequired => "Venue open times can't be empty.",
                ErrorCodes.VenueIsNotOpenNow => "Venue is not open now.",
                ErrorCodes.YouCannotDeleteThisRequest => "You can't delete this request since you are not the creator.",
                ErrorCodes.CantActivateVenueInAnInactiveCityOrArea => "Can't activate this venue because it's located in a deactivated city or area, please reactivate the chosen city or area, or change the location.",
                #endregion

                #region Loyalty
                ErrorCodes.InvalidLoyaltyCode => "Loyalty Not Redeemed.",
                ErrorCodes.LoyaltyCannotBeRedeemRightNow_InActive => "This loyalty cannot be redeemed right now, it is inactive.",
                ErrorCodes.LoyaltyCannotBeRedeemRightNow_NotValidTime => "This is not a valid time to redeem this loyalty.",
                ErrorCodes.LoyaltyCannotBeRedeemRightNow_InApplicable => "Cannot redeem this loyalty right now.",
                #endregion

                #region Venue Booking
                ErrorCodes.InvalidBookingDate => "Invalid Booking Date.",
                ErrorCodes.CantCancelAfterBookingDate => "Could not cancel your booking after its date has passed.",
                ErrorCodes.CantDeleteRemindersForThisBooking => "An error occured while trying to cancel your booking, could not remove reminder notifications.",
                ErrorCodes.CantSetReminderForOldBooking => "Couldn't set a reminder for old bookings.",
                ErrorCodes.CantSetReminder => "Couldn't set a reminder, you're not the owner of this booking.",
                ErrorCodes.CantShareRedeemedTicket => "Ticket is not shareable because it has been redeemed.",
                #endregion

                #region Event 
                ErrorCodes.EventNotFound => "Event was not found.",
                ErrorCodes.CantActivateEventAssignedToInactiveVenue => "Couldn't activate this event because the chosen venue is inactive.",
                ErrorCodes.UnavailableEvent => "Event is currently unavailable.",
                ErrorCodes.CantActivateEventInAnInactiveCityOrArea => "Can't activate this event because it's located in a deactivated city or area, please reactivate the chosen city or area, or change the location.",
                #endregion

                #region Event Booking
                ErrorCodes.TotalAmountIsNotCorrect => "Total amount is not correct, please try again.",
                ErrorCodes.Telr_TransactionError => "Transaction error, please try again later.",
                ErrorCodes.Telr_RequestTimeOut => "Transaction time out, please try again later.",
                ErrorCodes.TicketsSoldOut => "The package selected is sold out.",
                ErrorCodes.InvalidCode => "Invalid code.",
                ErrorCodes.InvalidTicket => "This ticket is not available for scanning",
                ErrorCodes.TicketDoesNotExist => "This ticket does not exist",
                ErrorCodes.TicketNotFound => "Ticket not found",
                ErrorCodes.InvalidSecret => "Invalid secret.",
                ErrorCodes.TicketUsedBefore => "Ticket has been used before.",
                ErrorCodes.CouldNotReceiveSharedTicket => "Couldn't receive the shared ticket.",
                ErrorCodes.UnableToCancelEventBooking => "Unable to cancel this booking because it's not paid",
                ErrorCodes.PackageTicketNumberHasZeroRemaining => "Can not decrease total tickets while remaining tickets 0",
                ErrorCodes.InvalidPackageTicketNumber => "Invalid ticket number",
                #endregion

                #region Terms And Condition
                ErrorCodes.TermAndConditionAlreadyExists => "This term and condition already exist, Please try again with a different description.",
                #endregion
                _ => code.ToString(),
            };
        }

    }
}
