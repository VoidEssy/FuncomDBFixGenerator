# Context
Funcom released an update which (according to community theory) tries to resolve the missing barkeeps (rootcause: change of file path) by doing a string look up for relevant entry. This change lead to Conan Exiles servers doing it for any and all mods with missing / unresolved entries and that can take a long time.
Servers that aren't freshly wiped could face 30-90 second look-ups PER ENTRY. So startup of 5-10min turns into 60+ min.
We have analyzed logs provided by community + our own servers and found a pattern which points to offending entries as a result a regex was created an incorporated into this little program. Reason why it's a program is because different servers will have different offending messages, some long standing servers even have Funcom's own Siptah DLC entries causing the loads. So it's impossible to make a "one size fits all" script but the pattern is consistent.
By analyzing the logs using regex pattern matching we extract offending entries from the logs and generated an appropriately sized clean-up / wipe-script for the server whose log is being analyzed.
**NOTE:** Regex doesn't yet capture ALL CASES as some seem to have a different pattern but it does capture the most common cases that majority of servers deal with and this software wil evolve as needed.

# FuncomDBFixGenerator

1. Download Zip.
1. Extract to a folder (preferably isolated one).
1. Make sure LogStreaming and LogPackageName aren't switched off.
1. Start up your server and let it load fully. (Yes it might take an hour).
1. Shut down your server.
1. Copy-paste ConanSandbox.log file (Unaltered just the file itself not the contents) into the same folder as the executable.
1. Run it.
1. It will Generate "GeneratedSql.sql" file.
1. Open the file
1. Copy-paste the content into your SQLite management tool
1. Follow the How to execute a **WIPE SCRIPT Guide** below


## How to execute WIPE SCRIPTS (And view / use SQL)
### BASIC TOP LEVEL FLOW:
1. Back-up DB
1. Execute Script
1. Fire Up Server
1. Login See if you crash / have issues
1. Roll Back the DB
1. Execute Script
1. Delete the Mod
1. Fire Up Your server

### HOW TO EXECUTE:
1. Download an SQLite manipulation software, I personally use SQLite Studio it's simple and light weight https://sqlitestudio.pl/
1. Shut down your server
1. Target your game.db that is located in saved folder of your server. (I personally recommend you make a backup copy also if you're using cloud hosting download the DB to your PC first, do the operations and then swap them out, remember to keep your backup)
1.  Copy paste the script IN FULL into the query field, if there is none just right click any table and choose "GENERATE SELECT" and replace the content that was generated for you with the content of said script.
1. Due to settings (You can explore them yourself) it might execute SQL queries only under "CURSOR" so just in case make sure to CTRL + A the whole script (Aka fully select it) 
1. Hit the Green Play Button and read the output below, if it was not red? But just displays execution time / affected row count, congrats you're done.
