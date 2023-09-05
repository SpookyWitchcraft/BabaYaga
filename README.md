## Welcome to BabaYaga!

### What's this all about?

This is the repository for BabaYaga (the bot portion of the Spooky Witchcraft project) written in F#.

### Additional Details

Currently BabaYaga offers the following commands:
* !coinflip
    * This command will return to the user either heads or tails.
    * Example: _!coinflip_
* !roll {number of dice}__d__{sides}
    * This command allows users to roll x number of y-sided dice.
        * The bot will return what each individual die rolled as well as the sum.
    * Example: _!roll 3d11_
    * Special note:  Please notice the letter _d_ between the number of dice and sides as it is required.
* !trivia
    * This command triggers the bot to ask a single trivia question.
        * Anyone may answer the question.
        * The question will time out at twenty seconds if no correct answer is given.
        * At ten seconds a hint is given.
    * Example: _!trivia_
* !chatgpt {question}
    * This command allows users to query [ChatGPT's API](https://platform.openai.com/docs/introduction) with simple queries.
    * Example: _!chatgpt in under 10 words tell me the atomic number of Mercury_
* !marvel {character name}
    * This command allows users to use [Marvel's API](https://developer.marvel.com/docs) to search for a brief description of a character.
    * Example: _!marvel wolverine_
    * Speacial note:  Marvel's API does not have a description for every (most?) characters.
* !report {report text}
    * This command allows users to create tickets/issues in GitHub if they encounter an issue.  The current issues can be found [here](https://github.com/SpookyWitchcraft/BabaYaga/issues).
    * Example: _!report question #3839 has an incorrect answer_

### Fun Fact
The atomic number of [Mercury](https://en.wikipedia.org/wiki/Mercury_(element)) is 80.
