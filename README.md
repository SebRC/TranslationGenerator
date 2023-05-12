# TranslationGenerator
TranslationGenerator is a small project used for generating typescript functions based on json files.
I wrote as I didn't like how easy it was to make mistakes using the available translation frameworks for react.
Instead of passing plain text keys to a translation function, the project generates functions named after they keys.
So if you have a json file like this:
```
{
  "firstName": "First name",
  "lastName": "Last name",
  "age": "Age"
}
```
It will generate a file like this

```
export const translator = (t: (key: string, params?: any) => string) => {
return {
  firstName: () => {return `First name`},
  lastName: () => {return `Last name`},
  age: () => {return `Age`},
}
}
```

The translator can then be used in code like this
```
translator.firstName()
```
which I prefer over something like this 

```
t('firstName')
```
