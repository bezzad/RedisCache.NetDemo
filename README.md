# Redis Cache in .Net Demo
The cache is the memory storage that is used to store the frequent access data into the temporary storage, it will improve the performance drastically and avoid the unnecessary database hit and store frequently used data into the buffer whenever we need it.

## Types of Cache
Basically, there are two types of caching .NET Core supports

1. In-Memory Caching
2. Distributed Caching

When we use In-Memory Cache then in that case data is stored in the application server memory and whenever we need then we fetch data from that and use it wherever we need it. And in Distributed Caching there are many third-party mechanisms like Redis and many others. But in this section, we look into the Redis Cache in detail and how it works in the .NET Core

## Distributed Caching
Basically, in the distributed cachin,g data are stored and shared between multiple servers
Also, it’s easy to improve scalability and performance of the application after managing the load between multiple servers when we use multi-tenant application
Suppose, In the future, if one server is crashed and restarted then the application does not have any impact because multiple servers are as per our need if we want
Redis is the most popular cache which is used by many companies nowadays to improve the performance and scalability of the application. So, we are going to discuss Redis and usage one by one.

## Redis Cache
Redis is an Open Source (BSD Licensed) in-memory Data Structure store used as a database.
Basically, it is used to store the frequently used and some static data inside the cache and use and reserve that as per user requirement.
There are many data structures present in the Redis which we are able to use like List, Set, Hashing, Stream, and many more to store the data.

## Redis vs. In-Memory caching in single instance benchmark

![Redis vs. InMemory](https://raw.githubusercontent.com/bezzad/RedisCache.NetDemo/main/img/Redis%20vs.%20MemoryCache%20-%20Single%20Instance.png)

## Installation of Redis Cache with docker

### Step 1
Install docker on your OS.

### Step 2 
Open bash and type below commands:
```cmd
$ docker pull redis:latest
$ docker run --name redis -p 6379:6379 -d redis:latest
```

Test is redis running:
```cmd
$ docker exec -it redis redis-cli
$ ping
```

## Implementation of Redis Cache using .NET Web API

### Step 1

Open this .NET Web API project and restore the following `NuGet` Packages which need step by step in this application.

```cmd
Swashbuckle.AspNetCore
StackExchange.Redis
```

### Step 2

Build this app and call API methods. So, trace `CacheService` codes to know which you need about this sample.



## Redis vs. MemoryCache Benchmark in Single instance
Test add and read time from local server memory
Sync    Redis = 128 * MemoryCache
Async   Redis = 186 * MemoryCache

## Redis vs. MemoryCache Benchmark in 16 instance
Test add and read time from local server memory with parallel 16 instance
Sync    Redis = 128 * MemoryCache
Async   Redis = 186 * MemoryCache