### Connecting to the Server

For each bot that is playing, 

### Bot Registration

Bot registration is done through a typical HTTPS API, using the `/register` route. The format is as follows:

```
Method: POST
URL: http://localhost:5000/register?name={name}
```

This endpoint returns a text response in the following format:

```json
{
    'portNumber': PORT_NUMBER
}
```

This exact port number must then be used when you connect to the server via a socket. See [Connecting to the Server](#connecting-to-the-server)

**NOTE**: 