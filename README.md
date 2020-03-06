We will create a simple application with dotnet-core

we will use the images created here kubernetes_chat_api_example [Chat-api-example](https://github.com/OktaySavdi/kubernetes_chat_api_example)


#  Build

       docker build -t chatapiproject -f Dockerfile .

# Run
    docker run -d -p 5000:80 --name myapp chatapiproject

# Call
    curl http://localhost:5000/api/swagger
