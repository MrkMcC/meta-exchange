To host the application container locally, navigate to the repository's root directory and run

```
docker build . -t meta-exchange-api
```
```
docker run -it --rm -p 32775:8080 --name MetaExchange meta-exchange-api  
```
The port 32775 is arbitrary and can be changed.

Visit ``http://localhost:32775/`` to view the Swagger documentation.

The API endpoint can be accessed like this:
```
http://localhost:32775/BTC/Buy/0.25
http://localhost:32775/BTC/Sell/0.25
```
