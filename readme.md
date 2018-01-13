# Helper library to work with SMS / text messages

### How can I calculate the number of required SMS parts for a concatenated text message?

    var helpers = new SmsHelpers();
    var parts = helpers.CountSmsParts(mymessage);

### How can I detect encoding required to send a text message?

    var helpers = new SmsHelpers();
    var parts = helpers.GetEncoding(mymessage);

### How can I use the library with dependency injection / IoC?

You only need to register our interface with your DI/IoC provider like this:

    public void RegisterDependencies(IServiceCollection services)
    {
        services.AddTransient(typeof(ISmsHelpers), typeof(SmsHelpers));
    }

### How can I normalize new line characters?

    var helpers = new SmsHelpers();
    var parts = helpers.NormalizeNewLines(mymessage);

