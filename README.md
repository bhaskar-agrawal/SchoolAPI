# SchoolAPI
SchoolAPI is the fun project to learn about API development in .Net. The API has simple RBAC where a teacher has access to update the marks of individual student while a student can just check their marks. 

# API Signature:

1. [POST] /schoolapi/auth/signup

Body: {
  "username": "ashish",
  "password": "123456",
  "role": "Teacher"
}

Response: 201 
{
    "userName": "ashish",
    "role": "Teacher"
}

2. [POST] /schoolapi/auth/login

Body: {
  "UserName": "ashish",
  "Password": "123456"
}

Response: 200
<Auth token is shared in string format>

3. [POST] /schoolapi/marks

Body: {
  "StudentName": "abhishek",
  "SubjectName": "Science",
  "marks": 32
}

//abhishek is already registered as student.

Response: 200 ok

4. [GET] schoolapi/marks?studentName=abhishek

Response: 
Body: [
    {
        "name": "Science",
        "marks": 32
    },
    {
        "name": "hindi",
        "marks": 32
    },
    {
        "name": "english",
        "marks": 32
    }
]
--------
# Docs and Links:
1. Dependency Injection: https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection
2. azure data explorer: https://learn.microsoft.com/en-us/dotnet/api/system.data.idatareader?view=net-8.0
3. Asynchronous programming: https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/task-asynchronous-programming-model
4. Simple course on pluralsight for the same: https://app.pluralsight.com/library/courses/asp-dot-net-core-6-web-api-fundamentals/table-of-contents

--------

# Future scope:
1. Integrate SQL DB.
2. Add custom metrics.
3. Deploy code using docker and kubernetes. 
