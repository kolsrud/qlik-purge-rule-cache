# qlik-purge-rule-cache

Tool for emptying the cache used for storing rule evaluations in the repository.

```
Usage:   PurgeRuleCache.exe --ntlm   <url>
         PurgeRuleCache.exe --direct <url> <port> [<userDir> <userId>]
Example: PurgeRuleCache.exe --ntlm   https://my.server.url
         PurgeRuleCache.exe --direct https://localhost 4242
         PurgeRuleCache.exe --direct https://my.server.url 4242 MyUserDir MyUserId
```
