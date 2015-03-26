# DeleteRec
Utility to remove windows directories which are very deep (paths exceeding 260 chars in length).

On Windows, if you end up with deep directory structures (paths longer than 260 chars) and you try to delete them from explorer or command line, the delete will fail. For example, npm often creates deeply nested structures which can leave you in this situation. You can use this utility to remove these deep directory structures or as a template to implement other operations (like move or copy).
