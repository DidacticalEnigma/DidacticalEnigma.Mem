Didactical Enigma.Mem
=================

Simple translation memory server

Currently it assumes the source language is Japanese and target language is English, though if needed, it could be generalized to other languages.

A single project contains many translation units, each one has source text and target text, and may have an associated context with it. Context stores a piece of textual or binary data, or both.

Each translation unit has a correlation id, which can store an identifier, unique to the project, which can be used to correlate a specific translation unit with an external resource or database.

Configuration
-------------

`DatabaseConfiguration:ConnectionString` describes the [connection string](https://www.npgsql.org/doc/connection-string-parameters.html) to be used to connect to a PostgreSQL database.

`MeCabConfiguration:PathToDictionary` must be directed to the directory where [MeCab's IPADIC dictionary](https://github.com/DidacticalEnigma/DidacticalEnigma-Data/tree/master/mecab/ipadic) is located.

`AuthConfiguration:Authority` and `AuthConfiguration:Audience` describe the parameters of the OAuth2 authentication provider. DidacticalEnigma.Mem does JWT token based authentication, and authorizes based on presence on of the following claims of type `permissions`:

- `modify:contexts` - Can add and remove contexts
- `modify:projects` - Can add and remove projects
- `modify:translations` - Can add, modify and delete translations
- `read:contexts` - Can read the data behind a context
- `read:listOfProjects` - Can enumerate all projects in the application
- `read:translations` - Can read translations

Note that in addition to the permissions in the token, `AuthConfiguration:AnonymousUsersCanReadTranslations` and `AuthConfiguration:AnonymousUsersCanReadContexts` control whether anonymous users, without providing any kind of token, are allowed to access translations and contexts.

Installation
------------

(assumes Debian 11 on the target server)

The provided installation script sets up the application running behind Nginx reverse proxy, with HTTPS provided by Let's Encrypt's Certbot, connecting to the local PostgreSQL instance. The script was tested with a clean Debian install, but should work properly even in case if Nginx, Certbot, or PostgreSQL are already installed.

From the main directory, run

```
dotnet ef migrations script -o Installation/initial-migration.sql -p DidacticalEnigma.Mem/DidacticalEnigma.Mem.csproj
dotnet publish -c Release -r linux-x64 --self-contained false -o Installation/DidacticalEnigma.Mem
```

Edit the `Installation/debian.sh` file and fill in the parameters.

Copy the `Installation` folder to the target server, on the target server, change directory to the newly copied `Installation` one, and run 

```
sudo ./debian.sh
```