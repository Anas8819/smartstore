namespace SmartStore.PayTabs.Services
{
    public enum PayTabEnum
    {
        None = 0,
        ShoppingCart,
        Address,
        PaymentMethod,
        OrderReviewData,
        ShippingMethod,
        MiniShoppingCart,

        /// <summary>
        /// Amazon Pay button clicked
        /// </summary>
        PayButtonHandler,

        /// <summary>
        /// Display authentication button on login page
        /// </summary>
        AuthenticationPublicInfo
    }

    public enum PayTabsTransactionType
    {
        None = 0,
        Authorize,
        AuthorizeAndCapture
    }

    public enum PayTabsAuthorizeMethod
    {
        Omnichronous = 0,
        Asynchronous,
        Synchronous
    }

    public enum PayTabsSaveDataType
    {
        None = 0,
        OnlyIfEmpty,
        Always
    }

    public enum PayTabsDataFetchingType
    {
        None = 0,
        Ipn,
        Polling
    }

    public enum PayTabsResultType
    {
        None = 0,
        PluginView,
        Redirect,
        Unauthorized
    }

    public enum PayTabsMessage
    {
        MessageTyp = 0,
        MessageId,
        AuthorizationID,
        CaptureID,
        RefundID,
        ReferenceID,
        State,
        StateUpdate,
        Fee,
        AuthorizedAmount,
        CapturedAmount,
        RefundedAmount,
        CaptureNow,
        Creation,
        Expiration
    }
}