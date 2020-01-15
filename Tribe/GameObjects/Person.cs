using System;
using System.Collections.Generic;
using System.Drawing;

namespace Tribe
{
    public enum Sex { Male, Female };
    public enum EffectState { Boost, Normal, Detriment }
    public enum Need { Health, Hunger, Social, Lust, Tiredness, Boredom, JobFullfilment, Warmth }

    public class BirthEventArgs : EventArgs
    {
        public Person Child { get; }
        public BirthEventArgs(Person child) { Child = child; }
    }

    public class ItemDropEventArgs : EventArgs
    {
        public bool InHouse { get; }
        public int Count { get; }
        public ItemType Type { get; }

        public ItemDropEventArgs(bool inHouse, int count, ItemType type)
        {
            InHouse = inHouse;
            Count = count;
            Type = type;
        }
    }


    public class Person : Entity, IGetData
    {
        public static readonly int ageSeconds = 5;
        public static readonly int carryCapacity = 5;
        public static readonly int sleepSeconds = 10;
        public static readonly int napSeconds = 4;

        public static readonly int mateRange = 10;
        public static readonly int mateTime = 2;

        private static readonly int gestationSeconds = 25;   // Gestation time in seconds.
        private static readonly int laborSeconds = 8;
        private static double defaultSpeed = 2.5;
        private static readonly double detrimentThreshold = 0.98; // Random double higher than this will get detriment attribute.
        private static readonly double boostThreshold = 0.2;  // Random double lower than this will get boost attribute.
        private static readonly Color pregnantColor = Color.FromArgb(255, 0, 230);

        private static readonly List<string> maleNames = new List<string> { "Douglas", "Roger", "Jonathan", "Ralph", "Nicholas", "Benjamin", "Bruce", "Harry", "Wayne", "Steve", "Howard", "Ernest", "Phillip", "Todd", "Craig", "Alan", "Philip", "Earl", "Danny", "Bryan", "Stanley", "Leonard", "Nathan", "Manuel", "Rodney", "Marvin", "Vincent", "Jeffery", "Jeff", "Chad", "Jacob", "Alfred", "Bradley", "Herbert", "Frederick", "Edwin", "Don", "Ricky", "Randall", "Barry", "Bernard", "Leroy", "Marcus", "Theodore", "Clifford", "Miguel", "Jim", "Tom", "Calvin", "Bill", "Lloyd", "Derek", "Warren", "Darrell", "Jerome", "Floyd", "Alvin", "Tim", "Gordon", "Greg", "Jorge", "Dustin", "Pedro", "Derrick", "Zachary", "Herman", "Glen", "Hector", "Ricardo", "Rick", "Brent", "Ramon", "Gilbert", "Marc", "Reginald", "Ruben", "Nathaniel", "Rafael", "Edgar", "Milton", "Raul", "Ben", "Chester", "Duane", "Franklin", "Brad", "Ron", "Roland", "Arnold", "Harvey", "Jared", "Erik", "Darryl", "Neil", "Javier", "Fernando", "Clinton", "Ted", "Mathew", "Tyrone", "Darren", "Lance", "Kurt", "Allan", "Nelson", "Guy", "Clayton", "Hugh", "Max", "Dwayne", "Dwight", "Armando", "Felix", "Everett", "Ian", "Wallace", "Ken", "Bob", "Alfredo", "Alberto", "Dave", "Ivan", "Byron", "Isaac", "Morris", "Clifton", "Willard", "Ross", "Andy", "Salvador", "Kirk", "Sergio", "Seth", "Kent", "Terrance", "Eduardo", "Terrence", "Enrique", "Wade", "Stuart", "Fredrick", "Arturo", "Alejandro", "Nick", "Luther", "Wendell", "Jeremiah", "Julius", "Otis", "Trevor", "Oliver", "Luke", "Homer", "Gerard", "Doug", "Kenny", "Hubert", "Lyle", "Matt", "Alfonso", "Orlando", "Rex", "Carlton", "Ernesto", "Neal", "Pablo", "Lorenzo", "Omar", "Wilbur", "Grant", "Horace", "Roderick", "Abraham", "Willis", "Rickey", "Andres", "Cesar", "Johnathan", "Malcolm", "Rudolph", "Damon", "Kelvin", "Preston", "Alton", "Archie", "Marco", "Wm", "Pete", "Randolph", "Garry", "Geoffrey", "Jonathon", "Felipe", "Gerardo", "Ed", "Dominic", "Delbert", "Colin", "Guillermo", "Earnest", "Lucas", "Benny", "Spencer", "Rodolfo", "Myron", "Edmund", "Garrett", "Salvatore", "Cedric", "Lowell", "Gregg", "Sherman", "Wilson", "Sylvester", "Roosevelt", "Israel", "Jermaine", "Forrest", "Wilbert", "Leland", "Simon", "Clark", "Irving", "Bryant", "Owen", "Rufus", "Woodrow", "Kristopher", "Mack", "Levi", "Marcos", "Gustavo", "Jake", "Lionel", "Gilberto", "Clint", "Nicolas", "Ismael", "Orville", "Ervin", "Dewey", "Al", "Wilfred", "Josh", "Hugo", "Ignacio", "Caleb", "Tomas", "Sheldon", "Erick", "Stewart", "Doyle", "Darrel", "Rogelio", "Terence", "Santiago", "Alonzo", "Elias", "Bert", "Elbert", "Ramiro", "Conrad", "Noah", "Grady", "Phil", "Cornelius", "Lamar", "Rolando", "Clay", "Percy", "Dexter", "Bradford", "Darin", "Amos", "Moses", "Irvin", "Saul", "Roman", "Randal", "Timmy", "Darrin", "Winston", "Brendan", "Abel", "Dominick", "Boyd", "Emilio", "Elijah", "Domingo", "Emmett", "Marlon", "Emanuel", "Jerald", "Edmond", "Emil", "Dewayne", "Will", "Otto", "Teddy", "Reynaldo", "Bret", "Jess", "Trent", "Humberto", "Emmanuel", "Stephan", "Vicente", "Lamont", "Garland", "Miles", "Efrain", "Heath", "Rodger", "Harley", "Ethan", "Eldon", "Rocky", "Pierre", "Junior", "Freddy", "Eli", "Bryce", "Antoine", "Sterling", "Chase", "Grover", "Elton", "Cleveland", "Dylan", "Chuck", "Damian", "Reuben", "Stan", "August", "Leonardo", "Jasper", "Russel", "Erwin", "Benito", "Hans", "Monte", "Blaine", "Ernie", "Curt", "Quentin", "Agustin", "Murray", "Jamal", "Adolfo", "Harrison", "Tyson", "Burton", "Brady", "Elliott", "Wilfredo", "Bart", "Jarrod", "Vance", "Denis", "Damien", "Joaquin", "Harlan", "Desmond", "Elliot", "Darwin", "Gregorio", "Buddy", "Xavier", "Kermit", "Roscoe", "Esteban", "Anton", "Solomon", "Scotty", "Norbert", "Elvin", "Williams", "Nolan", "Rod", "Quinton", "Hal", "Brain", "Rob", "Elwood", "Kendrick", "Darius", "Moises", "Fidel", "Thaddeus", "Cliff", "Marcel", "Jackson", "Raphael", "Bryon", "Armand", "Alvaro", "Jeffry", "Dane", "Joesph", "Thurman", "Ned", "Rusty", "Monty", "Fabian", "Reggie", "Mason", "Graham", "Isaiah", "Vaughn", "Gus", "Loyd", "Diego", "Adolph", "Norris", "Millard", "Rocco", "Gonzalo", "Derick", "Rodrigo", "Wiley", "Rigoberto", "Alphonso", "Ty", "Noe", "Vern", "Reed", "Jefferson", "Elvis", "Bernardo", "Mauricio", "Hiram", "Donovan", "Basil", "Riley", "Nickolas", "Maynard", "Scot", "Vince", "Quincy", "Eddy", "Sebastian", "Federico", "Ulysses", "Heriberto", "Donnell", "Cole", "Davis", "Gavin", "Emery", "Ward", "Romeo", "Jayson", "Dante", "Clement", "Coy", "Maxwell", "Jarvis", "Bruno", "Issac", "Dudley", "Brock", "Sanford", "Carmelo", "Barney", "Nestor", "Stefan", "Donny", "Art", "Linwood", "Beau", "Weldon", "Galen", "Isidro", "Truman", "Delmar", "Johnathon", "Silas", "Frederic", "Dick", "Irwin", "Merlin", "Charley", "Marcelino", "Harris", "Carlo", "Trenton", "Kurtis", "Hunter", "Aurelio", "Winfred", "Vito", "Collin", "Denver", "Carter", "Leonel", "Emory", "Pasquale", "Mohammad", "Mariano", "Danial", "Landon", "Dirk", "Branden", "Adan", "Buford", "German", "Wilmer", "Emerson", "Zachery", "Fletcher", "Jacques", "Errol", "Dalton", "Monroe", "Josue", "Edwardo", "Booker", "Wilford", "Sonny", "Shelton", "Carson", "Theron", "Raymundo", "Daren", "Houston", "Robby", "Lincoln", "Genaro", "Bennett", "Octavio", "Cornell", "Hung", "Arron", "Antony", "Herschel", "Giovanni", "Garth", "Cyrus", "Cyril", "Ronny", "Lon", "Freeman", "Duncan", "Kennith", "Carmine", "Erich", "Chadwick", "Wilburn", "Russ", "Reid", "Myles", "Anderson", "Morton", "Jonas", "Forest", "Mitchel", "Mervin", "Zane", "Rich", "Jamel", "Lazaro", "Alphonse", "Randell", "Major", "Jarrett", "Brooks", "Abdul", "Luciano", "Seymour", "Eugenio", "Mohammed", "Valentin", "Chance", "Arnulfo", "Lucien", "Ferdinand", "Thad", "Ezra", "Aldo", "Rubin", "Royal", "Mitch", "Earle", "Abe", "Wyatt", "Marquis", "Lanny", "Kareem", "Jamar", "Boris", "Isiah", "Emile", "Elmo", "Aron", "Leopoldo", "Everette", "Josef", "Eloy", "Rodrick", "Reinaldo", "Lucio", "Jerrod", "Weston", "Hershel", "Barton", "Parker", "Lemuel", "Burt", "Jules", "Gil", "Eliseo", "Ahmad", "Nigel", "Efren", "Antwan", "Alden", "Margarito", "Coleman", "Dino", "Osvaldo", "Les", "Deandre", "Normand", "Kieth", "Trey", "Norberto", "Napoleon", "Jerold", "Fritz", "Rosendo", "Milford", "Christoper", "Alfonzo", "Lyman", "Josiah", "Brant", "Wilton", "Rico", "Jamaal", "Dewitt", "Brenton", "Olin", "Foster", "Faustino", "Claudio", "Judson", "Gino", "Edgardo", "Alec", "Tanner", "Jarred", "Donn", "Tad", "Prince", "Porfirio", "Odis", "Lenard", "Chauncey", "Tod", "Mel", "Marcelo", "Kory", "Augustus", "Keven", "Hilario", "Bud", "Sal", "Orval", "Mauro", "Zachariah", "Olen", "Anibal", "Milo", "Jed", "Dillon", "Amado", "Newton", "Lenny", "Richie", "Horacio", "Brice", "Mohamed", "Delmer", "Dario", "Reyes", "Mac", "Jonah", "Jerrold", "Robt", "Hank", "Rupert", "Rolland", "Kenton", "Damion", "Antone", "Waldo", "Fredric", "Bradly", "Kip", "Burl", "Walker", "Tyree", "Jefferey", "Ahmed", "Willy", "Stanford", "Oren", "Noble", "Moshe", "Mikel", "Enoch", "Brendon", "Quintin", "Jamison", "Florencio", "Darrick", "Tobias", "Hassan", "Giuseppe", "Demarcus", "Cletus", "Tyrell", "Lyndon", "Keenan", "Werner", "Geraldo", "Columbus", "Chet", "Bertram", "Markus", "Huey", "Hilton", "Dwain", "Donte", "Tyron", "Omer", "Isaias", "Hipolito", "Fermin", "Adalberto", "Bo", "Barrett", "Teodoro", "Mckinley", "Maximo", "Garfield", "Raleigh", "Lawerence", "Abram", "Rashad", "King", "Emmitt", "Daron", "Samual", "Miquel", "Eusebio", "Domenic", "Darron", "Buster", "Wilber", "Renato", "Jc", "Hoyt", "Haywood", "Ezekiel", "Chas", "Florentino", "Elroy", "Clemente", "Arden", "Neville", "Edison", "Deshawn", "Nathanial", "Jordon", "Danilo", "Claud", "Sherwood", "Raymon", "Rayford", "Cristobal", "Ambrose", "Titus", "Hyman", "Felton", "Ezequiel", "Erasmo", "Stanton", "Lonny", "Len", "Ike", "Milan", "Lino", "Jarod", "Herb", "Andreas", "Walton", "Rhett", "Palmer", "Douglass", "Cordell", "Oswaldo", "Ellsworth", "Virgilio", "Toney", "Nathanael", "Del", "Benedict", "Mose", "Johnson", "Isreal", "Garret", "Fausto", "Asa", "Arlen", "Zack", "Warner", "Modesto", "Francesco", "Manual", "Gaylord", "Gaston", "Filiberto", "Deangelo", "Michale", "Granville", "Wes", "Malik", "Zackary", "Tuan", "Eldridge", "Cristopher", "Cortez", "Antione", "Malcom", "Long", "Korey", "Jospeh", "Colton", "Waylon", "Von", "Hosea", "Shad", "Santo", "Rudolf", "Rolf", "Rey", "Renaldo", "Marcellus", "Lucius", "Kristofer", "Boyce", "Benton", "Hayden", "Harland", "Arnoldo", "Rueben", "Leandro", "Kraig", "Jerrell", "Jeromy", "Hobert", "Cedrick", "Arlie", "Winford", "Wally", "Luigi", "Keneth", "Jacinto", "Graig", "Franklyn", "Edmundo", "Sid", "Porter", "Leif", "Jeramy", "Buck", "Willian", "Vincenzo", "Shon", "Lynwood", "Jere", "Hai", "Elden", "Dorsey", "Darell", "Broderick", "Alonso", "Adam", "Vinnie" };
        private static readonly List<string> femaleNames = new List<string> { "Mary", "Patricia", "Linda", "Barbara", "Elizabeth", "Jennifer", "Maria", "Susan", "Margaret", "Dorothy", "Lisa", "Nancy", "Karen", "Betty", "Helen", "Sandra", "Donna", "Carol", "Ruth", "Sharon", "Michelle", "Laura", "Sarah", "Kimberly", "Deborah", "Jessica", "Shirley", "Cynthia", "Angela", "Melissa", "Brenda", "Amy", "Anna", "Rebecca", "Virginia", "Kathleen", "Pamela", "Martha", "Debra", "Amanda", "Stephanie", "Carolyn", "Christine", "Marie", "Janet", "Catherine", "Frances", "Ann", "Joyce", "Diane", "Alice", "Julie", "Heather", "Teresa", "Doris", "Gloria", "Evelyn", "Jean", "Cheryl", "Mildred", "Katherine", "Joan", "Ashley", "Judith", "Rose", "Janice", "Kelly", "Nicole", "Judy", "Christina", "Kathy", "Theresa", "Beverly", "Denise", "Tammy", "Irene", "Jane", "Lori", "Rachel", "Marilyn", "Andrea", "Kathryn", "Louise", "Sara", "Anne", "Jacqueline", "Wanda", "Bonnie", "Julia", "Ruby", "Lois", "Tina", "Phyllis", "Norma", "Paula", "Diana", "Annie", "Lillian", "Emily", "Robin", "Peggy", "Crystal", "Gladys", "Rita", "Dawn", "Connie", "Florence", "Tracy", "Edna", "Tiffany", "Carmen", "Rosa", "Cindy", "Grace", "Wendy", "Victoria", "Edith", "Kim", "Sherry", "Sylvia", "Josephine", "Thelma", "Shannon", "Sheila", "Ethel", "Ellen", "Elaine", "Marjorie", "Carrie", "Charlotte", "Monica", "Esther", "Pauline", "Emma", "Juanita", "Anita", "Rhonda", "Hazel", "Amber", "Eva", "Debbie", "April", "Leslie", "Clara", "Lucille", "Jamie", "Joanne", "Eleanor", "Valerie", "Danielle", "Megan", "Alicia", "Suzanne", "Michele", "Gail", "Bertha", "Darlene", "Veronica", "Jill", "Erin", "Geraldine", "Lauren", "Cathy", "Joann", "Lorraine", "Lynn", "Sally", "Regina", "Erica", "Beatrice", "Dolores", "Bernice", "Audrey", "Yvonne", "Annette", "June", "Samantha", "Marion", "Dana", "Stacy", "Ana", "Renee", "Ida", "Vivian", "Roberta", "Holly", "Brittany", "Melanie", "Loretta", "Yolanda", "Jeanette", "Laurie", "Katie", "Kristen", "Vanessa", "Alma", "Sue", "Elsie", "Beth", "Jeanne", "Vicki", "Carla", "Tara", "Rosemary", "Eileen", "Terri", "Gertrude", "Lucy", "Tonya", "Ella", "Stacey", "Wilma", "Gina", "Kristin", "Jessie", "Natalie", "Agnes", "Vera", "Willie", "Charlene", "Bessie", "Delores", "Melinda", "Pearl", "Arlene", "Maureen", "Colleen", "Allison", "Tamara", "Joy", "Georgia", "Constance", "Lillie", "Claudia", "Jackie", "Marcia", "Tanya", "Nellie", "Minnie", "Marlene", "Heidi", "Glenda", "Lydia", "Viola", "Courtney", "Marian", "Stella", "Caroline", "Dora", "Jo", "Vickie", "Mattie", "Terry", "Maxine", "Irma", "Mabel", "Marsha", "Myrtle", "Lena", "Christy", "Deanna", "Patsy", "Hilda", "Gwendolyn", "Jennie", "Nora", "Margie", "Nina", "Cassandra", "Leah", "Penny", "Kay", "Priscilla", "Naomi", "Carole", "Brandy", "Olga", "Billie", "Dianne", "Tracey", "Leona", "Jenny", "Felicia", "Sonia", "Miriam", "Velma", "Becky", "Bobbie", "Violet", "Kristina", "Toni", "Misty", "Mae", "Shelly", "Daisy", "Ramona", "Sherri", "Erika", "Katrina", "Claire", "Lindsey", "Lindsay", "Geneva", "Guadalupe", "Belinda", "Margarita", "Sheryl", "Cora", "Faye", "Ada", "Natasha", "Sabrina", "Isabel", "Marguerite", "Hattie", "Harriet", "Molly", "Cecilia", "Kristi", "Brandi", "Blanche", "Sandy", "Rosie", "Joanna", "Iris", "Eunice", "Angie", "Inez", "Lynda", "Madeline", "Amelia", "Alberta", "Genevieve", "Monique", "Jodi", "Janie", "Maggie", "Kayla", "Sonya", "Jan", "Lee", "Kristine", "Candace", "Fannie", "Maryann", "Opal", "Alison", "Yvette", "Melody", "Luz", "Susie", "Olivia", "Flora", "Shelley", "Kristy", "Mamie", "Lula", "Lola", "Verna", "Beulah", "Antoinette", "Candice", "Juana", "Jeannette", "Pam", "Kelli", "Hannah", "Whitney", "Bridget", "Karla", "Celia", "Latoya", "Patty", "Shelia", "Gayle", "Della", "Vicky", "Lynne", "Sheri", "Marianne", "Kara", "Jacquelyn", "Erma", "Blanca", "Myra", "Leticia", "Pat", "Krista", "Roxanne", "Angelica", "Johnnie", "Robyn", "Francis", "Adrienne", "Rosalie", "Alexandra", "Brooke", "Bethany", "Sadie", "Bernadette", "Traci", "Jody", "Kendra", "Jasmine", "Nichole", "Rachael", "Chelsea", "Mable", "Ernestine", "Muriel", "Marcella", "Elena", "Krystal", "Angelina", "Nadine", "Kari", "Estelle", "Dianna", "Paulette", "Lora", "Mona", "Doreen", "Rosemarie", "Angel", "Desiree", "Antonia", "Hope", "Ginger", "Janis", "Betsy", "Christie", "Freda", "Mercedes", "Meredith", "Lynette", "Teri", "Cristina", "Eula", "Leigh", "Meghan", "Sophia", "Eloise", "Rochelle", "Gretchen", "Cecelia", "Raquel", "Henrietta", "Alyssa", "Jana", "Kelley", "Gwen", "Kerry", "Jenna", "Tricia", "Laverne", "Olive", "Alexis", "Tasha", "Silvia", "Elvira", "Casey", "Delia", "Sophie", "Kate", "Patti", "Lorena", "Kellie", "Sonja", "Lila", "Lana", "Darla", "May", "Mindy", "Essie", "Mandy", "Lorene", "Elsa", "Josefina", "Jeannie", "Miranda", "Dixie", "Lucia", "Marta", "Faith", "Lela", "Johanna", "Shari", "Camille", "Tami", "Shawna", "Elisa", "Ebony", "Melba", "Ora", "Nettie", "Tabitha", "Ollie", "Jaime", "Winifred", "Kristie", "Marina", "Alisha", "Aimee", "Rena", "Myrna", "Marla", "Tammie", "Latasha", "Bonita", "Patrice", "Ronda", "Sherrie", "Addie", "Francine", "Deloris", "Stacie", "Adriana", "Cheri", "Shelby", "Abigail", "Celeste", "Jewel", "Cara", "Adele", "Rebekah", "Lucinda", "Dorthy", "Chris", "Effie", "Trina", "Reba", "Shawn", "Sallie", "Aurora", "Lenora", "Etta", "Lottie", "Kerri", "Trisha", "Nikki", "Estella", "Francisca", "Josie", "Tracie", "Marissa", "Karin", "Brittney", "Janelle", "Lourdes", "Laurel", "Helene", "Fern", "Elva", "Corinne", "Kelsey", "Ina", "Bettie", "Elisabeth", "Aida", "Caitlin", "Ingrid", "Iva", "Eugenia", "Christa", "Goldie", "Cassie", "Maude", "Jenifer", "Therese", "Frankie", "Dena", "Lorna", "Janette", "Latonya", "Candy", "Morgan", "Consuelo", "Tamika", "Rosetta", "Debora", "Cherie", "Polly", "Dina", "Jewell", "Fay", "Jillian", "Dorothea", "Nell", "Trudy", "Esperanza", "Patrica", "Kimberley", "Shanna", "Helena", "Carolina", "Cleo", "Stefanie", "Rosario", "Ola", "Janine", "Mollie", "Lupe", "Alisa", "Lou", "Maribel", "Susanne", "Bette", "Susana", "Elise", "Cecile", "Isabelle", "Lesley", "Jocelyn", "Paige", "Joni", "Rachelle", "Leola", "Daphne", "Alta", "Ester", "Petra", "Graciela", "Imogene", "Jolene", "Keisha", "Lacey", "Glenna", "Gabriela", "Keri", "Ursula", "Lizzie", "Kirsten", "Shana", "Adeline", "Mayra", "Jayne", "Jaclyn", "Gracie", "Sondra", "Carmela", "Marisa", "Rosalind", "Charity", "Tonia", "Beatriz", "Marisol", "Clarice", "Jeanine", "Sheena", "Angeline", "Frieda", "Lily", "Robbie", "Shauna", "Millie", "Claudette", "Cathleen", "Angelia", "Gabrielle", "Autumn", "Katharine", "Summer", "Jodie", "Staci", "Lea", "Christi", "Jimmie", "Justine", "Elma", "Luella", "Margret", "Dominique", "Socorro", "Rene", "Martina", "Margo", "Mavis", "Callie", "Bobbi", "Maritza", "Lucile", "Leanne", "Jeannine", "Deana", "Aileen", "Lorie", "Ladonna", "Willa", "Manuela", "Gale", "Selma", "Dolly", "Sybil", "Abby", "Lara", "Dale", "Ivy", "Dee", "Winnie", "Marcy", "Luisa", "Jeri", "Magdalena", "Ofelia", "Meagan", "Audra", "Matilda", "Leila", "Cornelia", "Bianca", "Simone", "Bettye", "Randi", "Virgie", "Latisha", "Barbra", "Georgina", "Eliza", "Leann", "Bridgette", "Rhoda", "Haley", "Adela", "Nola", "Bernadine", "Flossie", "Ila", "Greta", "Ruthie", "Nelda", "Minerva", "Lilly", "Terrie", "Letha", "Hilary", "Estela", "Valarie", "Brianna", "Rosalyn", "Earline", "Catalina", "Ava", "Mia", "Clarissa", "Lidia", "Corrine", "Alexandria", "Concepcion", "Tia", "Sharron", "Rae", "Dona", "Ericka", "Jami", "Elnora", "Chandra", "Lenore", "Neva", "Marylou", "Melisa", "Tabatha", "Serena", "Avis", "Allie", "Sofia", "Jeanie", "Odessa", "Nannie", "Harriett", "Loraine", "Penelope", "Milagros", "Emilia", "Benita", "Allyson", "Ashlee", "Tania", "Tommie", "Esmeralda", "Karina", "Eve", "Pearlie", "Zelma", "Malinda", "Noreen", "Tameka", "Saundra", "Hillary", "Amie", "Althea", "Rosalinda", "Jordan", "Lilia", "Alana", "Gay", "Clare", "Alejandra", "Elinor", "Michael", "Lorrie", "Jerri", "Darcy", "Earnestine", "Carmella", "Taylor", "Noemi", "Marcie", "Liza", "Annabelle", "Louisa", "Earlene", "Mallory", "Carlene", "Nita", "Selena", "Tanisha", "Katy", "Julianne", "John", "Lakisha", "Edwina", "Maricela", "Margery", "Kenya", "Dollie", "Roxie", "Roslyn", "Kathrine", "Nanette", "Charmaine", "Lavonne", "Ilene", "Kris", "Tammi", "Suzette", "Corine", "Kaye", "Jerry", "Merle", "Chrystal", "Lina", "Deanne", "Lilian", "Juliana", "Aline", "Luann", "Kasey", "Maryanne", "Evangeline", "Colette", "Melva", "Lawanda", "Yesenia", "Nadia", "Madge", "Kathie", "Eddie", "Ophelia", "Valeria", "Nona", "Mitzi", "Mari", "Georgette", "Claudine", "Fran", "Alissa", "Roseann", "Lakeisha", "Susanna", "Reva", "Deidre", "Chasity", "Sheree", "Carly", "James", "Elvia", "Alyce", "Deirdre", "Gena", "Briana", "Araceli", "Katelyn", "Rosanne", "Wendi", "Tessa", "Berta", "Marva", "Imelda", "Marietta", "Marci", "Leonor", "Arline", "Sasha", "Madelyn", "Janna", "Juliette", "Deena", "Aurelia", "Josefa", "Augusta", "Liliana", "Young", "Christian", "Lessie", "Amalia", "Savannah", "Anastasia", "Vilma", "Natalia", "Rosella", "Lynnette", "Corina", "Alfreda", "Leanna", "Carey", "Amparo", "Coleen", "Tamra", "Aisha", "Wilda", "Karyn", "Cherry", "Queen", "Maura", "Mai", "Evangelina", "Rosanna", "Hallie", "Erna", "Enid", "Mariana", "Lacy", "Juliet", "Jacklyn", "Freida", "Madeleine", "Mara", "Hester", "Cathryn", "Lelia", "Casandra", "Bridgett", "Angelita", "Jannie", "Dionne", "Annmarie", "Katina", "Beryl", "Phoebe", "Millicent", "Katheryn", "Diann", "Carissa", "Maryellen", "Liz", "Lauri", "Helga", "Gilda", "Adrian", "Rhea", "Marquita", "Hollie", "Tisha", "Tamera", "Angelique", "Francesca", "Britney", "Kaitlin", "Lolita", "Florine", "Rowena", "Reyna", "Twila", "Fanny", "Janell", "Ines", "Concetta", "Bertie", "Alba", "Brigitte", "Alyson", "Vonda", "Pansy", "Elba", "Noelle", "Letitia", "Kitty", "Deann", "Brandie", "Louella", "Leta", "Felecia", "Sharlene", "Lesa", "Beverley", "Robert", "Isabella", "Herminia", "Terra", "Celina", "Tori", "Octavia", "Jade", "Denice", "Germaine", "Sierra", "Michell", "Cortney", "Nelly", "Doretha", "Sydney", "Deidra", "Monika", "Lashonda", "Judi", "Chelsey", "Antionette", "Margot", "Bobby", "Adelaide", "Nan", "Leeann", "Elisha", "Dessie", "Libby", "Kathi", "Gayla", "Latanya", "Mina", "Mellisa", "Kimberlee", "Jasmin", "Renae", "Zelda", "Elda", "Ma", "Justina", "Gussie", "Emilie", "Camilla", "Abbie", "Rocio", "Kaitlyn", "Jesse", "Edythe", "Ashleigh", "Selina", "Lakesha", "Geri", "Allene", "Pamala", "Michaela", "Dayna", "Caryn", "Rosalia", "Sun", "Jacquline", "Rebeca", "Marybeth", "Krystle", "Iola", "Dottie", "Bennie", "Belle", "Aubrey", "Griselda", "Ernestina", "Elida", "Adrianne", "Demetria", "Delma", "Chong", "Jaqueline", "Destiny", "Arleen", "Virgina", "Retha", "Fatima", "Tillie", "Eleanore", "Cari", "Treva", "Birdie", "Wilhelmina", "Rosalee", "Maurine", "Latrice", "Yong", "Jena", "Taryn", "Elia", "Debby", "Maudie", "Jeanna", "Delilah", "Catrina", "Shonda", "Hortencia", "Theodora", "Teresita", "Robbin", "Danette", "Maryjane", "Freddie", "Delphine", "Brianne", "Nilda", "Danna", "Cindi", "Bess", "Iona", "Hanna", "Ariel", "Winona", "Vida", "Rosita", "Marianna", "William", "Racheal", "Guillermina", "Eloisa", "Celestine", "Caren", "Malissa", "Lona", "Chantel", "Shellie", "Marisela", "Leora", "Agatha", "Soledad", "Migdalia", "Ivette", "Christen", "Athena", "Janel", "Chloe", "Veda", "Pattie", "Tessie", "Tera", "Marilynn", "Lucretia", "Karrie", "Dinah", "Daniela", "Alecia", "Adelina", "Vernice", "Shiela", "Portia", "Merry", "Lashawn", "Devon", "Dara", "Tawana", "Oma", "Verda", "Christin", "Alene", "Zella", "Sandi", "Rafaela", "Maya", "Kira", "Candida", "Alvina", "Suzan", "Shayla", "Lyn", "Lettie", "Alva", "Samatha", "Oralia", "Matilde", "Madonna", "Larissa", "Vesta", "Renita", "India", "Delois", "Shanda", "Phillis", "Lorri", "Erlinda", "Cruz", "Cathrine", "Barb", "Zoe", "Isabell", "Ione", "Gisela", "Charlie", "Valencia", "Roxanna", "Mayme", "Kisha", "Ellie", "Mellissa", "Dorris", "Dalia", "Bella", "Annetta", "Zoila", "Reta", "Reina", "Lauretta", "Kylie", "Christal", "Pilar", "Charla", "Elissa", "Tiffani", "Tana", "Paulina", "Leota", "Breanna", "Jayme", "Carmel", "Vernell", "Tomasa", "Mandi", "Dominga", "Santa", "Melodie", "Lura", "Alexa", "Tamela", "Ryan", "Mirna", "Kerrie", "Venus", "Noel", "Felicita", "Cristy", "Carmelita", "Berniece", "Annemarie", "Tiara", "Roseanne", "Missy", "Cori", "Roxana", "Pricilla", "Kristal", "Jung", "Elyse", "Haydee", "Aletha", "Bettina", "Marge", "Gillian", "Filomena", "Charles", "Zenaida", "Harriette", "Caridad", "Vada", "Una", "Aretha", "Pearline", "Marjory", "Marcela", "Flor", "Evette", "Elouise", "Alina", "Trinidad", "David", "Damaris", "Catharine", "Carroll", "Belva", "Nakia", "Marlena", "Luanne", "Lorine", "Karon", "Dorene", "Danita", "Brenna", "Tatiana", "Sammie", "Louann", "Loren", "Julianna", "Andria", "Philomena", "Lucila", "Leonora", "Dovie", "Romona", "Mimi", "Jacquelin", "Gaye", "Tonja", "Misti", "Joe", "Gene", "Chastity", "Stacia", "Roxann", "Micaela", "Nikita", "Mei", "Velda", "Marlys", "Johnna", "Aura", "Lavern", "Ivonne", "Hayley", "Nicki", "Majorie", "Herlinda", "George", "Alpha", "Yadira", "Perla", "Gregoria", "Daniel", "Antonette", "Shelli", "Mozelle", "Mariah", "Joelle", "Cordelia", "Josette", "Chiquita", "Trista", "Louis", "Laquita", "Georgiana", "Candi", "Shanon", "Lonnie", "Hildegard", "Cecil", "Valentina", "Stephany", "Magda", "Karol", "Gerry", "Gabriella", "Tiana", "Roma", "Richelle", "Ray", "Princess", "Oleta", "Jacque", "Idella", "Alaina", "Suzanna", "Jovita", "Blair", "Tosha", "Raven", "Nereida", "Marlyn", "Kyla", "Joseph", "Delfina", "Tena", "Stephenie", "Sabina", "Nathalie", "Marcelle", "Gertie", "Darleen", "Thea", "Sharonda", "Shantel", "Belen", "Venessa", "Rosalina", "Ona", "Genoveva", "Corey", "Clementine", "Rosalba", "Renate", "Renata", "Mi", "Ivory", "Georgianna", "Floy", "Dorcas", "Ariana", "Tyra", "Theda", "Mariam", "Juli", "Jesica", "Donnie", "Vikki", "Verla", "Roselyn", "Melvina", "Jannette", "Ginny", "Debrah", "Corrie", "Asia", "Violeta", "Myrtis", "Latricia", "Collette", "Charleen", "Anissa", "Viviana", "Twyla", "Precious", "Nedra", "Latonia", "Lan", "Hellen", "Fabiola", "Annamarie", "Adell", "Sharyn", "Chantal", "Niki", "Maud", "Lizette", "Lindy", "Kia", "Kesha", "Jeana", "Danelle", "Charline", "Chanel", "Carrol", "Valorie", "Lia", "Dortha", "Cristal", "Sunny", "Leone", "Leilani", "Gerri", "Debi", "Andra", "Keshia", "Ima", "Eulalia", "Easter", "Dulce", "Natividad", "Linnie", "Kami", "Georgie", "Catina", "Brook", "Alda", "Winnifred", "Sharla", "Ruthann", "Meaghan", "Magdalene", "Lissette", "Adelaida", "Venita", "Trena", "Shirlene", "Shameka", "Elizebeth", "Dian", "Shanta", "Mickey", "Latosha", "Carlotta", "Windy", "Soon", "Rosina", "Mariann", "Leisa", "Jonnie", "Dawna", "Cathie", "Billy", "Astrid", "Sidney", "Laureen", "Janeen", "Holli", "Fawn", "Vickey", "Teressa", "Shante", "Rubye", "Marcelina", "Chanda", "Cary", "Terese", "Scarlett", "Marty", "Marnie", "Lulu", "Lisette", "Jeniffer", "Elenor", "Dorinda", "Donita", "Carman", "Bernita", "Altagracia", "Aleta", "Adrianna", "Zoraida", "Ronnie", "Nicola", "Lyndsey", "Kendall", "Janina", "Chrissy", "Ami", "Starla", "Phylis", "Phuong", "Kyra", "Charisse", "Blanch", "Sanjuanita", "Rona", "Nanci", "Marilee", "Maranda", "Cory", "Brigette", "Sanjuana", "Marita", "Kassandra", "Joycelyn", "Ira", "Felipa", "Chelsie", "Bonny", "Mireya", "Lorenza", "Kyong", "Ileana", "Candelaria", "Tony", "Toby", "Sherie", "Ok", "Mark", "Lucie", "Leatrice", "Lakeshia", "Gerda", "Edie", "Bambi", "Marylin", "Lavon", "Hortense", "Garnet", "Evie", "Tressa", "Shayna", "Lavina", "Kyung", "Jeanetta", "Sherrill", "Shara", "Phyliss", "Mittie", "Anabel", "Alesia", "Thuy", "Tawanda", "Richard", "Joanie", "Tiffanie", "Lashanda", "Karissa", "Enriqueta", "Daria", "Daniella", "Corinna", "Alanna", "Abbey", "Roxane", "Roseanna", "Magnolia", "Lida", "Kyle", "Joellen", "Era", "Coral", "Carleen", "Tresa", "Peggie", "Novella", "Nila", "Maybelle", "Jenelle", "Carina", "Nova", "Melina", "Marquerite", "Margarette", "Josephina", "Evonne", "Devin", "Cinthia", "Albina", "Toya", "Tawnya", "Sherita", "Santos", "Myriam", "Lizabeth", "Lise", "Keely", "Jenni", "Giselle", "Cheryle", "Ardith", "Ardis", "Alesha", "Adriane", "Shaina", "Linnea", "Karolyn", "Hong", "Florida", "Felisha", "Dori", "Darci", "Artie", "Armida", "Zola", "Xiomara", "Vergie", "Shamika", "Nena", "Nannette", "Maxie", "Lovie", "Jeane", "Jaimie", "Inge", "Farrah", "Elaina", "Caitlyn", "Starr", "Felicitas", "Cherly", "Caryl", "Yolonda", "Yasmin", "Teena", "Prudence", "Pennie", "Nydia", "Mackenzie", "Orpha", "Marvel", "Lizbeth", "Laurette", "Jerrie", "Hermelinda", "Carolee", "Tierra", "Mirian", "Meta", "Melony", "Kori", "Jennette", "Jamila", "Ena", "Anh", "Yoshiko", "Susannah", "Salina", "Rhiannon", "Joleen", "Cristine", "Ashton", "Aracely", "Tomeka", "Shalonda", "Marti", "Lacie", "Kala", "Jada", "Ilse", "Hailey", "Brittani", "Zona", "Syble", "Sherryl", "Randy", "Nidia", "Marlo", "Kandice", "Kandi", "Deb", "Dean", "America", "Alycia", "Tommy", "Ronna", "Norene", "Mercy", "Jose", "Ingeborg", "Giovanna", "Gemma", "Christel", "Audry", "Zora", "Vita", "Van", "Trish", "Stephaine", "Shirlee", "Shanika", "Melonie", "Mazie", "Jazmin", "Inga", "Hoa", "Hettie", "Geralyn", "Fonda", "Estrella", "Adella", "Su", "Sarita", "Rina", "Milissa", "Maribeth", "Golda", "Evon", "Ethelyn", "Enedina", "Cherise", "Chana", "Velva", "Tawanna", "Sade", "Mirta", "Li", "Karie", "Jacinta", "Elna", "Davina", "Cierra", "Ashlie", "Albertha", "Tanesha", "Stephani", "Nelle", "Mindi", "Lu", "Lorinda", "Larue", "Florene", "Demetra", "Dedra", "Ciara", "Chantelle", "Ashly", "Suzy", "Rosalva", "Noelia", "Lyda", "Leatha", "Krystyna", "Kristan", "Karri", "Darline", "Darcie", "Cinda", "Cheyenne", "Cherrie", "Awilda", "Almeda", "Rolanda", "Lanette", "Jerilyn", "Gisele", "Evalyn", "Cyndi", "Cleta", "Carin", "Zina", "Zena", "Velia", "Tanika", "Paul", "Charissa", "Thomas", "Talia", "Margarete", "Lavonda", "Kaylee", "Kathlene", "Jonna", "Irena", "Ilona", "Idalia", "Candis", "Candance", "Brandee", "Anitra", "Alida", "Sigrid", "Nicolette", "Maryjo", "Linette", "Hedwig", "Christiana", "Cassidy", "Alexia", "Tressie", "Modesta", "Lupita", "Lita", "Gladis", "Evelia", "Davida", "Cherri", "Cecily", "Ashely", "Annabel", "Agustina", "Wanita", "Shirly", "Rosaura", "Hulda", "Eun", "Bailey", "Yetta", "Verona", "Thomasina", "Sibyl", "Shannan", "Mechelle", "Lue", "Leandra", "Lani", "Kylee", "Kandy", "Jolynn", "Ferne", "Eboni", "Corene", "Alysia", "Zula", "Nada", "Moira", "Lyndsay", "Lorretta", "Juan", "Jammie", "Hortensia", "Gaynell", "Cameron", "Adria", "Vina", "Vicenta", "Tangela", "Stephine", "Norine", "Nella", "Liana", "Leslee", "Kimberely", "Iliana", "Glory", "Felica", "Emogene", "Elfriede", "Eden", "Eartha", "Carma", "Bea", "Ocie", "Marry", "Lennie", "Kiara", "Jacalyn", "Carlota", "Arielle", "Yu", "Star", "Otilia", "Kirstin", "Kacey", "Johnetta", "Joey", "Joetta", "Jeraldine", "Jaunita", "Elana", "Dorthea", "Cami", "Amada", "Adelia", "Vernita", "Tamar", "Siobhan", "Renea", "Rashida", "Ouida", "Odell", "Nilsa", "Meryl", "Kristyn", "Julieta", "Danica", "Breanne", "Aurea", "Anglea", "Sherron", "Odette", "Malia", "Lorelei", "Lin", "Leesa", "Kenna", "Kathlyn", "Fiona", "Charlette", "Suzie", "Shantell", "Sabra", "Racquel", "Myong", "Mira", "Martine", "Lucienne", "Lavada", "Juliann", "Johnie", "Elvera", "Delphia", "Clair", "Christiane", "Charolette", "Carri", "Augustine", "Asha", "Angella", "Paola", "Ninfa", "Leda", "Lai", "Eda", "Sunshine", "Stefani", "Shanell", "Palma", "Machelle", "Lissa", "Kecia", "Kathryne", "Karlene", "Julissa", "Jettie", "Jenniffer", "Hui", "Corrina", "Christopher", "Carolann", "Alena", "Tess", "Rosaria", "Myrtice", "Marylee", "Liane", "Kenyatta", "Judie", "Janey", "In", "Elmira", "Eldora", "Denna", "Cristi", "Cathi", "Zaida", "Vonnie", "Viva", "Vernie", "Rosaline", "Mariela", "Luciana", "Lesli", "Karan", "Felice", "Deneen", "Adina", "Wynona", "Tarsha", "Sheron", "Shasta", "Shanita", "Shani", "Shandra", "Randa", "Pinkie", "Paris", "Nelida", "Marilou", "Lyla", "Laurene", "Laci", "Joi", "Janene", "Dorotha", "Daniele", "Dani", "Carolynn", "Carlyn", "Berenice", "Ayesha", "Anneliese", "Alethea", "Thersa", "Tamiko", "Rufina", "Oliva", "Mozell", "Marylyn", "Madison", "Kristian", "Kathyrn", "Kasandra", "Kandace", "Janae", "Gabriel", "Domenica", "Debbra", "Dannielle", "Chun", "Buffy", "Barbie", "Arcelia", "Aja", "Zenobia", "Sharen", "Sharee", "Patrick", "Page", "My", "Lavinia", "Kum", "Kacie", "Jackeline", "Huong", "Felisa", "Emelia", "Eleanora", "Cythia", "Cristin", "Clyde", "Claribel", "Caron", "Anastacia", "Zulma", "Zandra", "Yoko", "Tenisha", "Susann", "Sherilyn", "Shay", "Shawanda", "Sabine", "Romana", "Mathilda", "Linsey", "Keiko", "Joana", "Isela", "Gretta", "Georgetta", "Eugenie", "Dusty", "Desirae", "Delora", "Corazon", "Antonina", "Anika", "Willene", "Tracee", "Tamatha", "Regan", "Nichelle", "Mickie", "Maegan", "Luana", "Lanita", "Kelsie", "Edelmira", "Bree", "Afton", "Teodora", "Tamie", "Shena", "Meg", "Linh", "Keli", "Kaci", "Danyelle", "Britt", "Arlette", "Albertine", "Adelle", "Tiffiny", "Stormy", "Simona", "Numbers", "Nicolasa", "Nichol", "Nia", "Nakisha", "Mee", "Maira", "Loreen", "Kizzy", "Johnny", "Jay", "Fallon", "Christene", "Bobbye", "Anthony", "Ying", "Vincenza", "Tanja", "Rubie", "Roni", "Queenie", "Margarett", "Kimberli", "Irmgard", "Idell", "Hilma", "Evelina", "Esta", "Emilee", "Dennise", "Dania", "Carl", "Carie", "Antonio", "Wai", "Sang", "Risa", "Rikki", "Particia", "Mui", "Masako", "Mario", "Luvenia", "Loree", "Loni", "Lien", "Kevin", "Gigi", "Florencia", "Dorian", "Denita", "Dallas", "Chi", "Billye", "Alexander", "Tomika", "Sharita", "Rana", "Nikole", "Neoma", "Margarite", "Madalyn", "Lucina", "Laila", "Kali", "Jenette", "Gabriele", "Evelyne", "Elenora", "Clementina", "Alejandrina", "Zulema", "Violette", "Vannessa", "Thresa", "Retta", "Pia", "Patience", "Noella", "Nickie", "Jonell", "Delta", "Chung", "Chaya", "Camelia", "Bethel", "Anya", "Andrew", "Thanh", "Suzann", "Spring", "Shu", "Mila", "Lilla", "Laverna", "Keesha", "Kattie", "Gia", "Georgene", "Eveline", "Estell", "Elizbeth", "Vivienne", "Vallie", "Trudie", "Stephane", "Michel", "Magaly", "Madie", "Kenyetta", "Karren", "Janetta", "Hermine", "Harmony", "Drucilla", "Debbi", "Celestina", "Candie", "Britni", "Beckie", "Amina", "Zita", "Yun", "Yolande", "Vivien", "Vernetta", "Trudi", "Sommer", "Pearle", "Patrina", "Ossie", "Nicolle", "Loyce", "Letty", "Larisa", "Katharina", "Joselyn", "Jonelle", "Jenell", "Iesha", "Heide", "Florinda", "Florentina", "Flo", "Elodia", "Dorine", "Brunilda", "Brigid", "Ashli", "Ardella", "Twana", "Thu", "Tarah", "Sung", "Shea", "Shavon", "Shane", "Serina", "Rayna", "Ramonita", "Nga", "Margurite", "Lucrecia", "Kourtney", "Kati", "Jesus", "Jesenia", "Diamond", "Crista", "Ayana", "Alica", "Alia", "Suellen", "Romelia", "Rachell", "Piper", "Olympia", "Michiko", "Kathaleen", "Jolie", "Jessi", "Janessa", "Hana", "Ha", "Elease", "Carletta", "Britany", "Shona", "Salome", "Rosamond", "Regena", "Raina", "Ngoc", "Nelia", "Louvenia", "Lesia", "Latrina", "Laticia", "Larhonda", "Jina", "Jacki", "Hollis", "Holley", "Emmy", "Deeann", "Coretta", "Arnetta", "Velvet", "Thalia", "Shanice", "Neta", "Mikki", "Micki", "Lonna", "Leana", "Lashunda", "Kiley", "Joye", "Jacqulyn", "Ignacia", "Hyun", "Hiroko", "Henry", "Henriette", "Elayne", "Delinda", "Darnell", "Dahlia", "Coreen", "Consuela", "Conchita", "Celine", "Babette", "Ayanna", "Anette", "Albertina", "Skye", "Shawnee", "Shaneka", "Quiana", "Pamelia", "Min", "Merri", "Merlene", "Margit", "Kiesha", "Kiera", "Kaylene", "Jodee", "Jenise", "Erlene", "Emmie", "Else", "Daryl", "Dalila", "Daisey", "Cody", "Casie", "Belia", "Babara", "Versie", "Vanesa", "Shelba", "Shawnda", "Sam", "Norman", "Nikia", "Naoma", "Marna", "Margeret", "Madaline", "Lawana", "Kindra", "Jutta", "Jazmine", "Janett", "Hannelore", "Glendora", "Gertrud", "Garnett", "Freeda", "Frederica", "Florance", "Flavia", "Dennis", "Carline", "Beverlee", "Anjanette", "Valda", "Trinity", "Tamala", "Stevie", "Shonna", "Sha", "Sarina", "Oneida", "Micah", "Merilyn", "Marleen", "Lurline", "Lenna", "Katherin", "Jin", "Jeni", "Hae", "Gracia", "Glady", "Farah", "Eric", "Enola", "Ema", "Dominque", "Devona", "Delana", "Cecila", "Caprice", "Alysha", "Ali", "Alethia", "Vena", "Theresia", "Tawny", "Song", "Shakira", "Samara", "Sachiko", "Rachele", "Pamella", "Nicky", "Marni", "Mariel", "Maren", "Malisa", "Ligia", "Lera", "Latoria", "Larae", "Kimber", "Kathern", "Karey", "Jennefer", "Janeth", "Halina", "Fredia", "Delisa", "Debroah", "Ciera", "Chin", "Angelika", "Andree", "Altha", "Yen", "Vivan", "Terresa", "Tanna", "Suk", "Sudie", "Soo", "Signe", "Salena", "Ronni", "Rebbecca", "Myrtie", "Mckenzie", "Malika", "Maida", "Loan", "Leonarda", "Kayleigh", "France", "Ethyl", "Ellyn", "Dayle", "Cammie", "Brittni", "Birgit", "Avelina", "Asuncion", "Arianna", "Akiko", "Venice", "Tyesha", "Tonie", "Tiesha", "Takisha", "Steffanie", "Sindy", "Santana", "Meghann", "Manda", "Macie", "Lady", "Kellye", "Kellee", "Joslyn", "Jason", "Inger", "Indira", "Glinda", "Glennis", "Fernanda", "Faustina", "Eneida", "Elicia", "Dot", "Digna", "Dell", "Arletta", "Andre", "Willia", "Tammara", "Tabetha", "Sherrell", "Sari", "Refugio", "Rebbeca", "Pauletta", "Nieves", "Natosha", "Nakita", "Mammie", "Kenisha", "Kazuko", "Kassie", "Gary", "Earlean", "Daphine", "Corliss", "Clotilde", "Carolyne", "Bernetta", "Augustina", "Audrea", "Annis", "Annabell", "Yan", "Tennille", "Tamica", "Selene", "Sean", "Rosana", "Regenia", "Qiana", "Markita", "Macy", "Leeanne", "Laurine", "Kym", "Jessenia", "Janita", "Georgine", "Genie", "Emiko", "Elvie", "Deandra", "Dagmar", "Corie", "Collen", "Cherish", "Romaine", "Porsha", "Pearlene", "Micheline", "Merna", "Margorie", "Margaretta", "Lore", "Kenneth", "Jenine", "Hermina", "Fredericka", "Elke", "Drusilla", "Dorathy", "Dione", "Desire", "Celena", "Brigida", "Angeles", "Allegra", "Theo", "Tamekia", "Synthia", "Stephen", "Sook", "Slyvia", "Rosann", "Reatha", "Raye", "Marquetta", "Margart", "Ling", "Layla", "Kymberly", "Kiana", "Kayleen", "Katlyn", "Karmen", "Joella", "Irina", "Emelda", "Eleni", "Detra", "Clemmie", "Cheryll", "Chantell", "Cathey", "Arnita", "Arla", "Angle", "Angelic", "Alyse", "Zofia", "Thomasine", "Tennie", "Son", "Sherly", "Sherley", "Sharyl", "Remedios", "Petrina", "Nickole", "Myung", "Myrle", "Mozella", "Louanne", "Lisha", "Latia", "Lane", "Krysta", "Julienne", "Joel", "Jeanene", "Jacqualine", "Isaura", "Gwenda", "Earleen", "Donald", "Cleopatra", "Carlie", "Audie", "Antonietta", "Alise", "Alex", "Verdell", "Val", "Tyler", "Tomoko", "Thao", "Talisha", "Steven", "So", "Shemika", "Shaun", "Scarlet", "Savanna", "Santina", "Rosia", "Raeann", "Odilia", "Nana", "Minna", "Magan", "Lynelle", "Le", "Karma", "Joeann", "Ivana", "Inell", "Ilana", "Hye", "Honey", "Hee", "Gudrun", "Frank", "Dreama", "Crissy", "Chante", "Carmelina", "Arvilla", "Arthur", "Annamae", "Alvera", "Aleida", "Aaron", "Yee", "Yanira", "Vanda", "Tianna", "Tam", "Stefania", "Shira", "Perry", "Nicol", "Nancie", "Monserrate", "Minh", "Melynda", "Melany", "Matthew", "Lovella", "Laure", "Kirby", "Kacy", "Jacquelynn", "Hyon", "Gertha", "Francisco", "Eliana", "Christena", "Christeen", "Charise", "Caterina", "Carley", "Candyce", "Arlena", "Ammie", "Yang", "Willette", "Vanita", "Tuyet", "Tiny", "Syreeta", "Silva", "Scott", "Ronald", "Penney", "Nyla", "Michal", "Maurice", "Maryam", "Marya", "Magen", "Ludie", "Loma", "Livia", "Lanell", "Kimberlie", "Julee", "Donetta", "Diedra", "Denisha", "Deane", "Dawne", "Clarine", "Cherryl", "Bronwyn", "Brandon", "Alla", "Valery", "Tonda", "Sueann", "Soraya", "Shoshana", "Shela", "Sharleen", "Shanelle", "Nerissa", "Micheal", "Meridith", "Mellie", "Maye", "Maple", "Magaret", "Luis", "Lili", "Leonila", "Leonie", "Leeanna", "Lavonia", "Lavera", "Kristel", "Kathey", "Kathe", "Justin", "Julian", "Jimmy", "Jann", "Ilda", "Hildred", "Hildegarde", "Genia", "Fumiko", "Evelin", "Ermelinda", "Elly", "Dung", "Doloris", "Dionna", "Danae", "Berneice", "Annice", "Alix", "Verena", "Verdie", "Tristan", "Shawnna", "Shawana", "Shaunna", "Rozella", "Randee", "Ranae", "Milagro", "Lynell", "Luise", "Louie", "Loida", "Lisbeth", "Karleen", "Junita", "Jona", "Isis", "Hyacinth", "Hedy", "Gwenn", "Ethelene", "Erline", "Edward", "Donya", "Domonique", "Delicia", "Dannette", "Cicely", "Branda", "Blythe", "Bethann", "Ashlyn", "Annalee", "Alline", "Yuko", "Vella", "Trang", "Towanda", "Tesha", "Sherlyn", "Narcisa", "Miguelina", "Meri", "Maybell", "Marlana", "Marguerita", "Madlyn", "Luna", "Lory", "Loriann", "Liberty", "Leonore", "Leighann", "Laurice", "Latesha", "Laronda", "Katrice", "Kasie", "Karl", "Kaley", "Jadwiga", "Glennie", "Gearldine", "Francina", "Epifania", "Dyan", "Dorie", "Diedre", "Denese", "Demetrice", "Delena", "Darby", "Cristie", "Cleora", "Catarina", "Carisa", "Bernie", "Barbera", "Almeta", "Trula", "Tereasa", "Solange", "Sheilah", "Shavonne", "Sanora", "Rochell", "Mathilde", "Margareta", "Maia", "Lynsey", "Lawanna", "Launa", "Kena", "Keena", "Katia", "Jamey", "Glynda", "Gaylene", "Elvina", "Elanor", "Danuta", "Danika", "Cristen", "Cordie", "Coletta", "Clarita", "Carmon", "Brynn", "Azucena", "Aundrea", "Angele", "Yi", "Walter", "Verlie", "Verlene", "Tamesha", "Silvana", "Sebrina", "Samira", "Reda", "Raylene", "Penni", "Pandora", "Norah", "Noma", "Mireille", "Melissia", "Maryalice", "Laraine", "Kimbery", "Karyl", "Karine", "Kam", "Jolanda", "Johana", "Jesusa", "Jaleesa", "Jae", "Jacquelyne", "Irish", "Iluminada", "Hilaria", "Hanh", "Gennie", "Francie", "Floretta", "Exie", "Edda", "Drema", "Delpha", "Bev", "Barbar", "Assunta", "Ardell", "Annalisa", "Alisia", "Yukiko", "Yolando", "Wonda", "Wei", "Waltraud", "Veta", "Tequila", "Temeka", "Tameika", "Shirleen", "Shenita", "Piedad", "Ozella", "Mirtha", "Marilu", "Kimiko", "Juliane", "Jenice", "Jen", "Janay", "Jacquiline", "Hilde", "Fe", "Fae", "Evan", "Eugene", "Elois", "Echo", "Devorah", "Chau", "Brinda", "Betsey", "Arminda", "Aracelis", "Apryl", "Annett", "Alishia", "Veola", "Usha", "Toshiko", "Theola", "Tashia", "Talitha", "Shery", "Rudy", "Renetta", "Reiko", "Rasheeda", "Omega", "Obdulia", "Mika", "Melaine", "Meggan", "Martin", "Marlen", "Marget", "Marceline", "Mana", "Magdalen", "Librada", "Lezlie", "Lexie", "Latashia", "Lasandra", "Kelle", "Isidra", "Isa", "Inocencia", "Gwyn", "Francoise", "Erminia", "Erinn", "Dimple", "Devora", "Criselda", "Armanda", "Arie", "Ariane", "Angelo", "Angelena", "Allen", "Aliza", "Adriene", "Adaline", "Xochitl", "Twanna", "Tran", "Tomiko", "Tamisha", "Taisha", "Susy", "Siu", "Rutha", "Roxy", "Rhona", "Raymond", "Otha", "Noriko", "Natashia", "Merrie", "Melvin", "Marinda", "Mariko", "Margert", "Loris", "Lizzette", "Leisha", "Kaila", "Ka", "Joannie", "Jerrica", "Jene", "Jannet", "Janee", "Jacinda", "Herta", "Elenore", "Doretta", "Delaine", "Daniell", "Claudie", "China", "Britta", "Apolonia", "Amberly", "Alease", "Yuri", "Yuk", "Wen", "Waneta", "Ute", "Tomi", "Sharri", "Sandie", "Roselle", "Reynalda", "Raguel", "Phylicia", "Patria", "Olimpia", "Odelia", "Mitzie", "Mitchell", "Miss", "Minda", "Mignon", "Mica", "Mendy", "Marivel", "Maile", "Lynetta", "Lavette", "Lauryn", "Latrisha", "Lakiesha", "Kiersten", "Kary", "Josphine", "Jolyn", "Jetta", "Janise", "Jacquie", "Ivelisse", "Glynis", "Gianna", "Gaynelle", "Emerald", "Demetrius", "Danyell", "Danille", "Dacia", "Coralee", "Cher", "Ceola", "Brett", "Bell", "Arianne", "Aleshia", "Yung", "Williemae", "Troy", "Trinh", "Thora", "Tai", "Svetlana", "Sherika", "Shemeka", "Shaunda", "Roseline", "Ricki", "Melda", "Mallie", "Lavonna", "Latina", "Larry", "Laquanda", "Lala", "Lachelle", "Klara", "Kandis", "Johna", "Jeanmarie", "Jaye", "Hang", "Grayce", "Gertude", "Emerita", "Ebonie", "Clorinda", "Ching", "Chery", "Carola", "Breann", "Blossom", "Bernardine", "Becki", "Arletha", "Argelia", "Ara", "Alita", "Yulanda", "Yon", "Yessenia", "Tobi", "Tasia", "Sylvie", "Shirl", "Shirely", "Sheridan", "Shella", "Shantelle", "Sacha", "Royce", "Rebecka", "Reagan", "Providencia", "Paulene", "Misha", "Miki", "Marline", "Marica", "Lorita", "Latoyia", "Lasonya", "Kerstin", "Kenda", "Keitha", "Kathrin", "Jaymie", "Jack", "Gricelda", "Ginette", "Eryn", "Elina", "Elfrieda", "Danyel", "Cheree", "Chanelle", "Barrie", "Avery", "Aurore", "Annamaria", "Alleen", "Ailene", "Aide", "Yasmine", "Vashti", "Valentine", "Treasa", "Tory", "Tiffaney", "Sheryll", "Sharie", "Shanae", "Sau", "Raisa", "Pa", "Neda", "Mitsuko", "Mirella", "Milda", "Maryanna", "Maragret", "Mabelle", "Luetta", "Lorina", "Letisha", "Latarsha", "Lanelle", "Lajuana", "Krissy", "Karly", "Karena", "Jon", "Jessika", "Jerica", "Jeanelle", "January", "Jalisa", "Jacelyn", "Izola", "Ivey", "Gregory", "Euna", "Etha", "Drew", "Domitila", "Dominica", "Daina", "Creola", "Carli", "Camie", "Bunny", "Brittny", "Ashanti", "Anisha", "Aleen", "Adah", "Yasuko", "Winter", "Viki", "Valrie", "Tona", "Tinisha", "Thi", "Terisa", "Tatum", "Taneka", "Simonne", "Shalanda", "Serita", "Ressie", "Refugia", "Paz", "Olene", "Na", "Merrill", "Margherita", "Mandie", "Man", "Maire", "Lyndia", "Luci", "Lorriane", "Loreta", "Leonia", "Lavona", "Lashawnda", "Lakia", "Kyoko", "Krystina", "Krysten", "Kenia", "Kelsi", "Jude", "Jeanice", "Isobel", "Georgiann", "Genny", "Felicidad", "Eilene", "Deon", "Deloise", "Deedee", "Dannie", "Conception", "Clora", "Cherilyn", "Chang", "Calandra", "Berry", "Armandina", "Anisa", "Ula", "Timothy", "Tiera", "Theressa", "Stephania", "Sima", "Shyla", "Shonta", "Shera", "Shaquita", "Shala", "Sammy", "Rossana", "Nohemi", "Nery", "Moriah", "Melita", "Melida", "Melani", "Marylynn", "Marisha", "Mariette", "Malorie", "Madelene", "Ludivina", "Loria", "Lorette", "Loralee", "Lianne", "Leon", "Lavenia", "Laurinda", "Lashon", "Kit", "Kimi", "Keila", "Katelynn", "Kai", "Jone", "Joane", "Ji", "Jayna", "Janella", "Ja", "Hue", "Hertha", "Francene", "Elinore", "Despina", "Delsie", "Deedra", "Clemencia", "Carry", "Carolin", "Carlos", "Bulah", "Brittanie", "Bok", "Blondell", "Bibi", "Beaulah", "Beata", "Annita", "Agripina", "Virgen", "Valene", "Un", "Twanda", "Tommye", "Toi", "Tarra", "Tari", "Tammera", "Shakia", "Sadye", "Ruthanne", "Rochel", "Rivka", "Pura", "Nenita", "Natisha", "Ming", "Merrilee", "Melodee", "Marvis", "Lucilla", "Leena", "Laveta", "Larita", "Lanie", "Keren", "Ileen", "Georgeann", "Genna", "Genesis", "Frida", "Ewa", "Eufemia", "Emely", "Ela", "Edyth", "Deonna", "Deadra", "Darlena", "Chanell", "Chan", "Cathern", "Cassondra", "Cassaundra", "Bernarda", "Berna", "Arlinda", "Anamaria", "Albert", "Wesley", "Vertie", "Valeri", "Torri", "Tatyana", "Stasia", "Sherise", "Sherill", "Season", "Scottie", "Sanda", "Ruthe", "Rosy", "Roberto", "Robbi", "Ranee", "Quyen", "Pearly", "Palmira", "Onita", "Nisha", "Niesha", "Nida", "Nevada", "Nam", "Merlyn", "Mayola", "Marylouise", "Maryland", "Marx", "Marth", "Margene", "Madelaine", "Londa", "Leontine", "Leoma", "Leia", "Lawrence", "Lauralee", "Lanora", "Lakita", "Kiyoko", "Keturah", "Katelin", "Kareen", "Jonie", "Johnette", "Jenee", "Jeanett", "Izetta", "Hiedi", "Heike", "Hassie", "Harold", "Giuseppina", "Georgann", "Fidela", "Fernande", "Elwanda", "Ellamae", "Eliz", "Dusti", "Dotty", "Cyndy", "Coralie", "Celesta", "Argentina", "Alverta", "Xenia", "Wava", "Vanetta", "Torrie", "Tashina", "Tandy", "Tambra", "Tama", "Stepanie", "Shila", "Shaunta", "Sharan", "Shaniqua", "Shae", "Setsuko", "Serafina", "Sandee", "Rosamaria", "Priscila", "Olinda", "Nadene", "Muoi", "Michelina", "Mercedez", "Maryrose", "Marin", "Marcene", "Mao", "Magali", "Mafalda", "Logan", "Linn", "Lannie", "Kayce", "Karoline", "Kamilah", "Kamala", "Justa", "Joline", "Jennine", "Jacquetta", "Iraida", "Gerald", "Georgeanna", "Franchesca", "Fairy", "Emeline", "Elane", "Ehtel", "Earlie", "Dulcie", "Dalene", "Cris", "Classie", "Chere", "Charis", "Caroyln", "Carmina", "Carita", "Brian", "Bethanie", "Ayako", "Arica", "An", "Alysa", "Alessandra", "Akilah", "Adrien", "Zetta", "Youlanda", "Yelena", "Yahaira", "Xuan", "Wendolyn", "Victor", "Tijuana", "Terrell", "Terina", "Teresia", "Suzi", "Sunday", "Sherell", "Shavonda", "Shaunte", "Sharda", "Shakita", "Sena", "Ryann", "Rubi", "Riva", "Reginia", "Rea", "Rachal", "Parthenia", "Pamula", "Monnie", "Monet", "Michaele", "Melia", "Marine", "Malka", "Maisha", "Lisandra", "Leo", "Lekisha", "Lean", "Laurence", "Lakendra", "Krystin", "Kortney", "Kizzie", "Kittie", "Kera", "Kendal", "Kemberly", "Kanisha", "Julene", "Jule", "Joshua", "Johanne", "Jeffrey", "Jamee", "Han", "Halley", "Gidget", "Galina", "Fredricka", "Fleta", "Fatimah", "Eusebia", "Elza", "Eleonore", "Dorthey", "Doria", "Donella", "Dinorah", "Delorse", "Claretha", "Christinia", "Charlyn", "Bong", "Belkis", "Azzie", "Andera", "Aiko", "Adena", "Yer", "Yajaira", "Wan", "Vania", "Ulrike", "Toshia", "Tifany", "Stefany", "Shizue", "Shenika", "Shawanna", "Sharolyn", "Sharilyn", "Shaquana", "Shantay", "See", "Rozanne", "Roselee", "Rickie", "Remona", "Reanna", "Raelene", "Quinn", "Phung", "Petronila", "Natacha", "Nancey", "Myrl", "Miyoko", "Miesha", "Merideth", "Marvella", "Marquitta", "Marhta", "Marchelle", "Lizeth", "Libbie", "Lahoma", "Ladawn", "Kina", "Katheleen", "Katharyn", "Karisa", "Kaleigh", "Junie", "Julieann", "Johnsie", "Janean", "Jaimee", "Jackqueline", "Hisako", "Herma", "Helaine", "Gwyneth", "Glenn", "Gita", "Eustolia", "Emelina", "Elin", "Edris", "Donnette", "Donnetta", "Dierdre", "Denae", "Darcel", "Claude", "Clarisa", "Cinderella", "Chia", "Charlesetta", "Charita", "Celsa", "Cassy", "Cassi", "Carlee", "Bruna", "Brittaney", "Brande", "Billi", "Bao", "Antonetta", "Angla", "Angelyn", "Analisa", "Alane", "Wenona", "Wendie", "Veronique", "Vannesa", "Tobie", "Tempie", "Sumiko", "Sulema", "Sparkle", "Somer", "Sheba", "Shayne", "Sharice", "Shanel", "Shalon", "Sage", "Roy", "Rosio", "Roselia", "Renay", "Rema", "Reena", "Porsche", "Ping", "Peg", "Ozie", "Oretha", "Oralee", "Oda", "Nu", "Ngan", "Nakesha", "Milly", "Marybelle", "Marlin", "Maris", "Margrett", "Maragaret", "Manie", "Lurlene", "Lillia", "Lieselotte", "Lavelle", "Lashaunda", "Lakeesha", "Keith", "Kaycee", "Kalyn", "Joya", "Joette", "Jenae", "Janiece", "Illa", "Grisel", "Glayds", "Genevie", "Gala", "Fredda", "Fred", "Elmer", "Eleonor", "Debera", "Deandrea", "Dan", "Corrinne", "Cordia", "Contessa", "Colene", "Cleotilde", "Charlott", "Chantay", "Cecille", "Beatris", "Azalee", "Arlean", "Ardath", "Anjelica", "Anja", "Alfredia", "Aleisha", "Zada", "Yuonne", "Xiao", "Willodean", "Whitley", "Vennie", "Vanna", "Tyisha", "Tova", "Torie", "Tonisha", "Tilda", "Tien", "Temple", "Sirena", "Sherril", "Shanti", "Shan", "Senaida", "Samella", "Robbyn", "Renda", "Reita", "Phebe", "Paulita", "Nobuko", "Nguyet", "Neomi", "Moon", "Mikaela", "Melania", "Maximina", "Marg", "Maisie", "Lynna", "Lilli", "Layne", "Lashaun", "Lakenya", "Lael", "Kirstie", "Kathline", "Kasha", "Karlyn", "Karima", "Jovan", "Josefine", "Jennell", "Jacqui", "Jackelyn", "Hyo", "Hien", "Grazyna", "Florrie", "Floria", "Eleonora", "Dwana", "Dorla", "Dong", "Delmy", "Deja", "Dede", "Dann", "Crysta", "Clelia", "Claris", "Clarence", "Chieko", "Cherlyn", "Cherelle", "Charmain", "Chara", "Cammy", "Bee", "Arnette", "Ardelle", "Annika", "Amiee", "Amee", "Allena", "Yvone", "Yuki", "Yoshie", "Yevette", "Yael", "Willetta", "Voncile", "Venetta", "Tula", "Tonette", "Timika", "Temika", "Telma", "Teisha", "Taren", "Ta", "Stacee", "Shin", "Shawnta", "Saturnina", "Ricarda", "Pok", "Pasty", "Onie", "Nubia", "Mora", "Mike", "Marielle", "Mariella", "Marianela", "Mardell", "Many", "Luanna", "Loise", "Lisabeth", "Lindsy", "Lilliana", "Lilliam", "Lelah", "Leigha", "Leanora", "Lang", "Kristeen", "Khalilah", "Keeley", "Kandra", "Junko", "Joaquina", "Jerlene", "Jani", "Jamika", "Jame", "Hsiu", "Hermila", "Golden", "Genevive", "Evia", "Eugena", "Emmaline", "Elfreda", "Elene", "Donette", "Delcie", "Deeanna", "Darcey", "Cuc", "Clarinda", "Cira", "Chae", "Celinda", "Catheryn", "Catherin", "Casimira", "Carmelia", "Camellia", "Breana", "Bobette", "Bernardina", "Bebe", "Basilia", "Arlyne", "Amal", "Alayna", "Zonia", "Zenia", "Yuriko", "Yaeko", "Wynell", "Willow", "Willena", "Vernia", "Tu", "Travis", "Tora", "Terrilyn", "Terica", "Tenesha", "Tawna", "Tajuana", "Taina", "Stephnie", "Sona", "Sol", "Sina", "Shondra", "Shizuko", "Sherlene", "Sherice", "Sharika", "Rossie", "Rosena", "Rory", "Rima", "Ria", "Rheba", "Renna", "Peter", "Natalya", "Nancee", "Melodi", "Meda", "Maxima", "Matha", "Marketta", "Maricruz", "Marcelene", "Malvina", "Luba", "Louetta", "Leida", "Lecia", "Lauran", "Lashawna", "Laine", "Khadijah", "Katerine", "Kasi", "Kallie", "Julietta", "Jesusita", "Jestine", "Jessia", "Jeremy", "Jeffie", "Janyce", "Isadora", "Georgianne", "Fidelia", "Evita", "Eura", "Eulah", "Estefana", "Elsy", "Elizabet", "Eladia", "Dodie", "Dion", "Dia", "Denisse", "Deloras", "Delila", "Daysi", "Dakota", "Curtis", "Crystle", "Concha", "Colby", "Claretta", "Chu", "Christia", "Charlsie", "Charlena", "Carylon", "Bettyann", "Asley", "Ashlea", "Amira", "Ai", "Agueda", "Agnus", "Yuette", "Vinita", "Victorina", "Tynisha", "Treena", "Toccara", "Tish", "Thomasena", "Tegan", "Soila", "Shiloh", "Shenna", "Sharmaine", "Shantae", "Shandi", "September", "Saran", "Sarai", "Sana", "Samuel", "Salley", "Rosette", "Rolande", "Regine", "Otelia", "Oscar", "Olevia", "Nicholle", "Necole", "Naida", "Myrta", "Myesha", "Mitsue", "Minta", "Mertie", "Margy", "Mahalia", "Madalene", "Love", "Loura", "Lorean", "Lewis", "Lesha", "Leonida", "Lenita", "Lavone", "Lashell", "Lashandra", "Lamonica", "Kimbra", "Katherina", "Karry", "Kanesha", "Julio", "Jong", "Jeneva", "Jaquelyn", "Hwa", "Gilma", "Ghislaine", "Gertrudis", "Fransisca", "Fermina", "Ettie", "Etsuko", "Ellis", "Ellan", "Elidia", "Edra", "Dorethea", "Doreatha", "Denyse", "Denny", "Deetta", "Daine", "Cyrstal", "Corrin", "Cayla", "Carlita", "Camila", "Burma", "Bula", "Buena", "Blake", "Barabara", "Avril", "Austin", "Alaine", "Zana", "Wilhemina", "Wanetta", "Virgil", "Vi", "Veronika", "Vernon", "Verline", "Vasiliki", "Tonita", "Tisa", "Teofila", "Tayna", "Taunya", "Tandra", "Takako", "Sunni", "Suanne", "Sixta", "Sharell", "Seema", "Russell", "Rosenda", "Robena", "Raymonde", "Pei", "Pamila", "Ozell", "Neida", "Neely", "Mistie", "Micha", "Merissa", "Maurita", "Maryln", "Maryetta", "Marshall", "Marcell", "Malena", "Makeda", "Maddie", "Lovetta", "Lourie", "Lorrine", "Lorilee", "Lester", "Laurena", "Lashay", "Larraine", "Laree", "Lacresha", "Kristle", "Krishna", "Keva", "Keira", "Karole", "Joie", "Jinny", "Jeannetta", "Jama", "Heidy", "Gilberte", "Gema", "Faviola", "Evelynn", "Enda", "Elli", "Ellena", "Divina", "Dagny", "Collene", "Codi", "Cindie", "Chassidy", "Chasidy", "Catrice", "Catherina", "Cassey", "Caroll", "Carlena", "Candra", "Calista", "Bryanna", "Britteny", "Beula", "Bari", "Audrie", "Audria", "Ardelia", "Annelle", "Angila", "Alona", "Allyn" };
        private static readonly List<string> surnames = new List<string> { "Hildebrand", "Gof", "Bell", "Criswick", "Douglas", "Holdgate", "Dyer", "Littlewood", "Wentz", "Grisedale", "Rawcliff", "Nessen", "Frew", "Beadon", "Dickins", "Parrish", "Seargent", "Baskeyfield", "Dufour", "Parkinson", "Mostyn", "Earsome", "Fathwaite", "Wyatville", "Hazelford", "List", "Batie", "Woodgate", "Duttondurocher", "Rowell", "Penrith", "Merrin", "Smyth", "Twining", "Thelma", "Earle", "Consfielde", "Hewett", "Dealdeburgh", "Mann", "Clyde", "Thornborough", "Travis", "Dance", "Cameron", "Mcglynn", "Babbington", "Compton-Hall", "Sutherland", "Pollitt", "Smythe", "Carew", "Backman", "Slemons", "Macfle", "Maltby", "Symmons", "Longhurst", "Spencer", "Campy", "Fleischmann", "Longden", "Rokeby", "Cattermoul", "Humphrey", "Nanson", "Hewat", "Munsey", "Bone", "Nettleton", "Curry", "Dora", "Lovelace", "Burk", "Paisley", "Pounder", "Langstaff", "Tamlinson", "Mcdivitt", "Demarkfield", "Pettitt", "Rainge", "Ducharme", "Lindsay", "Faithwaite", "Sonelay", "Hay", "Standefer", "Offley", "Lunt", "Clare", "Dalgliesh", "Fleming", "Fold", "Oversby", "Obersby", "Willmott", "Redmond", "Gammie", "Munsey", "Rickens", "Humphries", "Starling", "Ncguffie", "Sparks", "Weston", "Cocciardi-Milne", "Wagnon", "Bridges", "Brennand", "Barth", "Potman", "Eckhold", "Swaffield", "Akester", "Other", "Isle", "Fulcher", "Keddie", "Rydall", "Eastgate", "Kedslie", "Hawden", "Waterton", "Bean", "Sherram", "Boyes", "Hewitt", "Worsham", "Raw", "Conner", "How", "Mcfarlin", "Spontheimer", "Everingham", "Macalpine", "Ashbury", "Nicol", "Burill", "Eastham", "Hallam", "Harriet", "Carmen", "Hands", "Bostin", "Medaris", "Adams", "Pringle", "Marshall", "Hardy", "Harries", "Stennett", "Thompson", "Sherrod", "Seaborn", "Wansker", "Milbanke", "Tinsley", "Graham", "Meikeljohn", "Morrell", "Mace", "Blenain", "Dehart", "Harkins", "Granger", "Scutts", "Hepburn", "Mckechnie", "Hutchins", "Lovelace", "Tattersall", "Stimson", "Welbourne", "Aumonier", "Dimascio", "Barter", "Motley", "Witte", "Melbourne", "Batterham", "Hadley", "Jagger", "Waldegrave", "Cowthwaite", "Tatersall", "Smyth", "Smith", "Clegg", "Dilahoy", "Hotson", "Shores", "Shearer", "Lengden", "Benning", "Crossland", "Townend", "Stones", "Huby", "Lambdin", "Langston", "Stinson", "Justice", "Chettle", "Bunyan", "Threet", "Poat", "Locke", "Presley", "Swafford", "Dyke", "Linfoot", "Landry", "Early", "Edmunds", "Lyster", "Wellesley", "Bent", "Marksbury", "Tailor", "Hunter", "Manly", "Farley", "Spurlock", "Washington", "Devenport", "Doherty", "Harvey", "Verdich", "Sutherland-Leveson-Gower", "Pieters", "Tylecote", "Guernsey", "Marbury", "Harley", "Loud", "Parsons", "Everest", "Dunkley", "Deserre", "Atkins", "Campin", "Lancaster", "Macleod", "Naddox", "Sanders", "Spooner", "Cropper", "Ramsbotham", "Townson", "Salter", "Meadows", "Goring", "Glendenning", "Valentine", "Crosskey", "Farish", "Church", "Ingalls", "Pettinger", "Barfield", "Coltart", "Merrin", "Hendry", "Reynar", "Backhouse", "Wight", "Hossack", "Flynn", "Dell", "Pallin", "Gers", "Guess", "Cragg", "Tunstall", "Barkman", "Wordsworth", "Restall", "Diggles", "Prue", "Nessen", "Bannister", "Gillyatt", "Aulas", "Hamer", "Earnshaw", "Desatge", "Dethoren", "Patching", "Short", "Holegate", "Pethick", "Hartwell", "Jocelyn", "Thulburn", "Bainbrigg", "Traynum", "Mulroney", "Berkley", "Tanger", "Braunsheidel" };

        public Item ActiveWeapon { get; private set; }

        public delegate void BirthHandler(Person person, BirthEventArgs e); // Event for the entity handler to listen for and add people to the list.
        public event BirthHandler GiveBirth;

        public event ItemEventHandler RequestItem;

        public delegate void ItemDroppedHandler(Person person, ItemDropEventArgs e);
        public event ItemDroppedHandler DroppedItem; // Person drops food off in the house.

        public event DataChangedHandler UpdateElement;
        public event EventHandler CancelData;

        public void OnCancelData()
        {
            EventHandler handler = CancelData;
            handler?.Invoke(this, new EventArgs());
        }

        public List<Item> Inventory { get; private set; }

        public double HealthNeed { get; private set; }
        
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public Sex Sex { get; protected set; }
        public Person Fetus { get; private set; }

        public ObjectData Data { get; protected set; }

        // Needs
        /* Social requires searching for nearby people. Rest will increase slowly over time.
         */
        public static readonly double hungerDelta = 0.010; // Player only needs these two so separate these from Person.UpdateNeeds();
        public static readonly double tirednessDelta = 0.002;
       
        // Hereditary Effects
        public EffectState EVision { get; private set; }
        public EffectState EMobility { get; private set; }
        public EffectState EMood { get; private set; }
        public EffectState EStrength { get; private set; }

        public double[] Attributes { get; private set; } = { 
            /*Bravery:*/0.50, /*Resilience:*/0.80, /*Strength:*/0.50, /*Mood:*/0.75, /*Immune System:*/0.90
        };

        public double Bravery { get => Attributes[0]; private set => Attributes[0] = value; } // Threshold to run from certain threats.
        public double Resilience { get => Attributes[1];  private set => Attributes[1] = value; } // Threshold to have a mental break.
        public override double Strength { get => Attributes[2];  protected set => Attributes[2] = value; } // Attack damage.
        public double Mood { get => Attributes[3];  private set => Attributes[3] = value; } // Mood: happiness, stress level.
        public double ImmuneSystemStrength { get => Attributes[4];  private set => Attributes[4] = value; }

        public bool HasStd { get; private set; } = false;
        public bool IsPregnant { get; private set; } = false;


        // Create a new person without specifying the position. Used for creating children and assigning the position at birth.
        public Person(string firstName, string lastName, Sex sex, bool randomEffects = false)
            : base(defaultSpeed, adultSize, 0, 0, GetBaseColor(sex, false), ageSeconds)
        {
            FirstName = firstName;
            LastName = lastName;
            Sex = sex;
            Inventory = new List<Item>();
            RandomizeAttributes();
            if (randomEffects)
                RandomizeEffects();

        }

        // Create a new person with randomly generated name by sex.
        public Person(Sex sex, int x, int y, bool randomEffects = false)
            : base(defaultSpeed, adultSize, x, y, GetBaseColor(sex, false), ageSeconds)
        {
            FirstName = sex == Sex.Male ? maleNames.RandomChoice(Utilities.Rng) : femaleNames.RandomChoice(Utilities.Rng);
            LastName = surnames.RandomChoice(Utilities.Rng);
            Sex = sex;
            Inventory = new List<Item>();
            RandomizeAttributes();
            if (randomEffects)
                RandomizeEffects();
        }

        public Person(string firstName, string lastName, Sex sex, double x, double y, bool randomEffects = false)
            : base(defaultSpeed, adultSize, x, y, GetBaseColor(sex, false), ageSeconds)
        {
            FirstName = firstName;
            LastName = lastName;
            Sex = sex;
            Inventory = new List<Item>();
            RandomizeAttributes();
            if (randomEffects)
                RandomizeEffects();

        }

        // Pass event information to any objects that are listening for Person's "IGetData" related event.
        private void OnUpdateElement(ChangeType type, string elementName, object value)
        {
            DataChangedHandler handler = UpdateElement;
            handler?.Invoke(this, new DataChangedArgs(type, elementName, value));
        }

        public override string ToString()
        {
            return FirstName + " " + LastName;
        }

        public override bool TrySetAction(EntityAction newAction, bool locked = false)
        {
            OnUpdateElement(ChangeType.UpdateElement, "Action", newAction != null ? newAction.GetName() : "None");
            return base.TrySetAction(newAction, locked);
        }

        public void ClearAction()
        {
            Action = null;
            IdleAction = null;
            OnUpdateElement(ChangeType.UpdateElement, "Action", "None");
        }

        // People have a specific damage response. They will attack or flee from their attacker. Then do Entity attack response.
        public override void TakeDamage(int damage, Entity attacker)
        {
            base.TakeDamage(damage, attacker);
            OnUpdateElement(ChangeType.UpdateElement, "Health", Health);
        }

        public int GetItemCount(ItemType type)
        {
            Item storedItem = Inventory.Find(p => p.Type == type);

            return (storedItem == null) ? 0 : storedItem.Amount;
        }

        // Return data for display.
        public ObjectData GetData()
        {
            return Data;
        }

        // Set values for each Person's data. This is shown on display panels.
        public void UpdateData()
        {
            // new Tuple<string, object>("", ),

            Data = new ObjectData(new List<Tuple<string, object>>()
            {
                new Tuple<string, object>("FirstName", FirstName),
                new Tuple<string, object>("LastName", LastName),
                new Tuple<string, object>("Health", GetHealth()),
                new Tuple<string, object>("Sex", Sex),
                new Tuple<string, object>("IsPregnant", IsPregnant ? "Yes" : "No"),
                new Tuple<string, object>("HasStd", HasStd),
                new Tuple<string, object>("Hunger", Hunger),
                new Tuple<string, object>("Social", Social),
                new Tuple<string, object>("Lust", Lust),
                new Tuple<string, object>("Tiredness", Tiredness),
                new Tuple<string, object>("Boredom", Boredom),
                new Tuple<string, object>("JobFullfilment", JobFullfilment),
                new Tuple<string, object>("Warmth", Warmth),
                new Tuple<string, object>("Action", Action != null ? Action.GetName() : "Null"), //
                new Tuple<string, object>("ActionLocked", ActionLocked) //
            }, 14);
        }

        // TODO: Organize this better. Only SetPosition depends on the item type.
        // Apply damage modifier from the weapon and add it to the RenderContext.
        public void SetActiveWeapon(Item item)
        {
            Damage += item.ModValue;
            ActiveWeapon = item;
            RenderContext.AddAccessory(item);

            if (item.Type == ItemType.Spear)
            {
                item.SetPosition(new OrderedPair<double>(-12, -5));
            }

        }

        public void RemoveActiveWeapon()
        {
            Damage -= ActiveWeapon.ModValue;
            RenderContext.RemoveAccessory(ActiveWeapon);
            ActiveWeapon = null;
        }

        // Try inventory as an expandable list first. If searching is too slow, switch to an array instead with global item counts for the size...
        public void AddItem(Item item)
        {
            Item carriedItem = Inventory.Find(p => p.Type == item.Type);

            if (item.IsWeapon)
            {
                SetActiveWeapon(item); // Set damage modifier and add the weapon to the Person's RenderContext.
            }

            if (carriedItem == null) // The person is not carrying an item that matches the item to add.
            {
                Inventory.Add(item);
                OnUpdateElement(ChangeType.NewElement, item.Type.ToString(), item.Amount);
            }
            else
            {
                carriedItem.AddAmount(item.Amount);
                OnUpdateElement(ChangeType.UpdateElement, item.Type.ToString(), carriedItem.Amount);
            }

            UpdateData();
        }

        

        public void RemoveItem(Item item, bool inHouse, bool addItemToWorld = true)
        {

            if (ActiveWeapon != null && item.Type == ActiveWeapon.Type)
            {
                RemoveActiveWeapon();
            }

            if (item.Amount == 0)
            {
                Inventory.Remove(item);
                return;
            }

            //Console.WriteLine($"{FirstName} dropping {item.Type}");

            Item carriedItem = Inventory.Find(p => p.Type == item.Type);

            if (carriedItem != null) // The person is carrying an item that matches the item to remove.
            {
                if (addItemToWorld)
                {
                    ItemDroppedHandler handler = DroppedItem;
                    handler?.Invoke(this, new ItemDropEventArgs(inHouse, item.Amount, item.Type));
                }

                carriedItem.TakeAmount(item.Amount);

                if (carriedItem.Amount <= 0)
                {
                    Inventory.Remove(carriedItem);
                    OnUpdateElement(ChangeType.RemoveElement, item.Type.ToString(), carriedItem.Amount);
                }
                else
                {
                    OnUpdateElement(ChangeType.UpdateElement, item.Type.ToString(), carriedItem.Amount);
                }

                UpdateData();
            }

        }

        public void Pickup(Item item)
        {
            //Console.WriteLine($"{FirstName} picking up item {item.ObjectID}");

            if (item.IsWeapon && ActiveWeapon != null)
            {
                Console.WriteLine("You can only pickup one weapon at a time");
                return;
            }

            int amountCarried = GetItemCount(item.Type);

            if (amountCarried <= carryCapacity)
            {
                int amountTaken = Math.Min(item.Amount, carryCapacity - amountCarried);

                if (amountTaken == 0)
                    return;

                item.TakeAmount(amountTaken);
                AddItem(new Item(0, 0, item.Type, amountTaken));
            }

        }

        // Removes all food that the person is carrying. NOTE: Might not work because DropItem removes items from the inventory if the value == 0...
        public void DropAllItems(bool inHouse)
        {
            for (int i = 0; i < Inventory.Count; i++)
            {
                RemoveItem(Inventory[i], inHouse);
            }
        }

        // Trigger RequestItem event to ask EntityController for items.
        private void OnRequestItem(ItemType type)
        {
            //Console.WriteLine($"Person {FirstName} calling OnRequestItem()");
            ItemEventHandler handler = RequestItem;
            handler?.Invoke(this, new ItemEventArgs(new Item(0, 0, type, 1)));
        }

        public void RequestItemByType(ItemType itemType)
        {
            OnRequestItem(itemType);
        }


        public void Eat()
        {
            Item carriedFood = Inventory.Find(p => p.Type == ItemType.Apple);

            if (carriedFood == null)
                return;
            else
            {
                carriedFood.TakeAmount(1);
                Hunger += 0.5;
                // OnUpdateElement(..."Hunger"...) necessary here because the need update call will overwrite this anyway.
            }
        }

        public override void ApplySleepDelta()
        {
            base.ApplySleepDelta();
            OnUpdateElement(ChangeType.UpdateElement, "Health", Health);
        }

        public void GiveStd() { HasStd = true; OnUpdateElement(ChangeType.UpdateElement, "HasStd", HasStd); }

        #region Generating Attributes
        private void RandomizeEffects()
        {
            EffectState[] states = new EffectState[4];
            for (int i = 0; i < 4; i++)
            {
                states[i] = GetRandomEffectState(boostThreshold, detrimentThreshold);
            }
            EVision = states[0];
            EMobility = states[1];
            EMood = states[2];
            EStrength = states[3];

            if (Utilities.Rng.NextDouble() < 0.01)
                HasStd = true;

            ApplyEffects();
        }

        // Random double < bottomCeiling will be boosted. Random double > topFloor will be detriment.
        private EffectState GetRandomEffectState(double bottomCeiling, double topFloor)
        {
            double rand = Utilities.Rng.NextDouble();
            if (rand < bottomCeiling) return EffectState.Boost;
            else if (rand >= topFloor) return EffectState.Detriment;
            else return EffectState.Normal;
        }

        private void ApplyEffects()
        {
            switch (EVision)
            {
                case EffectState.Boost: VisionRange += 20; break;
                case EffectState.Detriment: VisionRange -= 20; break;
            }
            switch (EMobility)
            {
                case EffectState.Boost: Speed += 0.5; break;
                case EffectState.Detriment: Speed -= 0.5; break;
            }
            switch (EMood)
            {
                case EffectState.Boost: Mood += 10; break;
                case EffectState.Detriment: Mood -= 10; break;
            }
            switch (EStrength)
            {
                case EffectState.Boost: Strength += 10; break;
                case EffectState.Detriment: Strength -= 10; break;
            }

            if (HasStd)
            {
                Speed -= 1;
                Mood -= 10;
            }
        }

        private void RandomizeAttributes()
        {
            for (int i = 0; i < Attributes.Length; i++)
            {
                Attributes[i] += Utilities.Rng.Next(-10, 11) * 0.01; // Randomize default attribute within +- 0.01.
            }
        }
        #endregion

        public static Color GetBaseColor(Sex sex, bool isPregnant)
        {
            if (sex == Sex.Male)
            {
                return Color.Blue;
            }
            else if (isPregnant)
            {
                return pregnantColor;
            }
            else
                return Color.Purple;
        }

        public bool AttackerInRange()
        {
            return Attacker != null && Math.Sqrt(Utilities.SquaredDistance(Position, Attacker.Position)) < VisionRange;
        }

        public override void SetBaseColor() => Color = GetBaseColor(Sex, IsPregnant);

        #region Reproduction
        public void ReproduceWith(Person mate)
        {
            if (this.Sex == mate.Sex)
                throw new ArgumentException("");

            if (!this.IsPregnant && !mate.IsPregnant)
            {
                string lastName;
                bool thisFemale;
                bool isGirl = Utilities.Rng.Next(0, 2) == 0;
                string firstName = isGirl ? femaleNames.RandomChoice(Utilities.Rng) : maleNames.RandomChoice(Utilities.Rng);
               
                double x, y;
                if (this.Sex == Sex.Male)
                {
                    lastName = this.LastName;
                    thisFemale = false;
                    x = mate.Position.X;
                    y = mate.Position.Y;
                }
                else
                {
                    lastName = mate.LastName;
                    thisFemale = true;
                    x = this.Position.X;
                    y = this.Position.Y;
                }

                Sex childSex = isGirl ? Sex.Female : Sex.Male;

                Person child = new Person(firstName, lastName, childSex);

                // Double "base" attributes applied via random crossover. Real genetics has all traits passed this way, but this should do.
                // Other hereditary effects will use an approximation of the punnett-square-dominant-repressive thing.
                int crossoverPoint = Utilities.Rng.Next(Attributes.Length);
                for (int i = 0; i < crossoverPoint; i++)
                {
                    if (Utilities.Rng.Next(0, 2) == 0) // Apply 50% chance of attribute from parent1 or parent2 for crossover attributes.
                        child.Attributes[i] = mate.Attributes[i];
                }
                for (int i = crossoverPoint; i < Attributes.Length; i++)
                {
                    child.Attributes[i] = this.Attributes[i];
                }

                // Vision
                if (this.EVision == mate.EVision)
                    child.EVision = this.EVision;
                else
                    EVision = GetRandomEffectState(boostThreshold, detrimentThreshold);
                // Mobility
                if (this.EVision == mate.EMobility)
                    child.EMobility = this.EMobility;
                else
                    EMobility = GetRandomEffectState(boostThreshold, detrimentThreshold);
                // Mood
                if (this.EMood == mate.EMood)
                    child.EMood = this.EMood;
                else
                    EMood = GetRandomEffectState(boostThreshold, detrimentThreshold);
                // Strength
                if (this.EStrength == mate.EStrength)
                    child.EMood = this.EMood;
                else
                    EStrength = GetRandomEffectState(boostThreshold, detrimentThreshold);

                // Decide if an std will be spread.
                if (this.HasStd & !mate.HasStd & Utilities.Rng.NextDouble() > mate.ImmuneSystemStrength)
                {
                    mate.HasStd = true;
                    mate.OnUpdateElement(ChangeType.UpdateElement, "HasStd", HasStd);
                }
                    
                if (mate.HasStd & !this.HasStd & Utilities.Rng.NextDouble() > this.ImmuneSystemStrength)
                {
                    this.HasStd = true;
                    OnUpdateElement(ChangeType.UpdateElement, "HasStd", HasStd);
                }
                    
                child.ApplyEffects();

                if (thisFemale)
                    this.Impregnate(child);
                else
                    mate.Impregnate(child);

                OnCreateEffect(EffectType.Love, true);
            }
        }

        // "this" will call MateWith as part of MateAction. This call will set mate's action to MateAction
        //  it will also do this.ReproduceWith(mate) to create a baby.
        public void MateWith(Person mate)
        {
            mate.TrySetAction(new WaitAction(mateTime), true);
        }

        public void Impregnate(Person child)
        {
            Color = pregnantColor;
            IsPregnant = true;
            OnUpdateElement(ChangeType.UpdateElement, "IsPregnant", IsPregnant ? "Yes" : "No");
            Fetus = child;
            //Console.WriteLine("Impregnate: {0}", FirstName);

            EntityAction action = new BirthAction(laborSeconds);
            OnScheduleEvent(this, new ScheduleEventArgs(gestationSeconds, action)); // Forward info to Entity.cs to raise the event.
            Speed -= 1;
        }


        public void Abort() { Fetus = null; SetBaseColor(); }

        private void SetLocation(int x, int y) { Position = new OrderedPair<double>(x, y); }

        public void ToggleSex()
        {
            if (Sex == Sex.Male)
                Sex = Sex.Female;
            else
                Sex = Sex.Male;

            Color = GetBaseColor(Sex, IsPregnant); // Update color after change.
            OnUpdateElement(ChangeType.UpdateElement, "Sex", Sex);
        }

        public void Birth()
        {
            //Console.WriteLine("{0} giving birth to {1}", FirstName, Fetus.FirstName);
            Fetus.SetLocation((int)Position.X, (int)Position.Y);
            SetBaseColor();
            Speed += 1;
            IsPregnant = false;
            OnUpdateElement(ChangeType.UpdateElement, "IsPregnant", IsPregnant ? "Yes" : "No");
            BirthHandler handler = GiveBirth; // Create copy of the event to allow the null check to work even during a race condition.
            handler?.Invoke(this, new BirthEventArgs(Fetus)); // Let the entity controller know to schedule an event.
        }
        #endregion

        #region Needs

        public override List<ActionUtility> GetAdvertisedActions(Entity entity, double mod)
        {
            var actionPairs = new List<ActionUtility>();

            if (entity is Person person)
            {
                if (this.IsAdult && person.IsAdult && this.Sex != person.Sex && !this.IsPregnant && !person.IsPregnant)
                    actionPairs.Add(
                        // One time need delta that has to build up over the duration of the action.
                        new ActionUtility(new MateAction(this, mateTime), new Tuple<Need, double>[]
                        {
                            ActionUtility.NewPair(Need.Lust, 0.025 * mod / mateTime),
                            ActionUtility.NewPair(Need.Social, 0.01 * mod / mateTime)
                        }
                        ));
                actionPairs.Add(
                    new ActionUtility(FollowThisAction, new Tuple<Need, double>[]
                    {
                            ActionUtility.NewPair(Need.Social, 0.0025 * (person.IsPregnant ? 5.0 : 1.0))
                    }));
            }
            
            else if (entity is Animal animal && animal.Type == AnimalType.Bear)
            {
                actionPairs.Add(
                    new ActionUtility(AttackThisAction, new Tuple<Need, double>[]
                    {
                        ActionUtility.NewPair(Need.Hunger, 0.5)
                    }));
            }

            return actionPairs;
        }

        // Dependent on Form1's timer interval and the current frame rate.
        // Need to be careful with stopping the needs at 1.0 because some of them may never be fulfilled.
        public virtual void UpdateNeeds()
        {
            Hunger = Math.Max(0.0, Hunger - hungerDelta);
            Social = Math.Max(0.0, Social - 0.001);
            Lust = Math.Max(0.0, Lust - 0.001);
            Tiredness = Math.Max(0.0, Tiredness - tirednessDelta);
            Boredom = Math.Max(0.0, Boredom - 0.001);
            JobFullfilment = Math.Max(0.0, JobFullfilment - 0.003);
            Warmth = Math.Max(0.0, Warmth - 0.001);

            OnUpdateElement(ChangeType.UpdateElement, "Hunger", Hunger);
            OnUpdateElement(ChangeType.UpdateElement, "Social", Social);
            OnUpdateElement(ChangeType.UpdateElement, "Lust", Lust);
            OnUpdateElement(ChangeType.UpdateElement, "Tiredness", Tiredness);
            OnUpdateElement(ChangeType.UpdateElement, "Boredom", Boredom);
            OnUpdateElement(ChangeType.UpdateElement, "JobFullfilment", JobFullfilment);
            OnUpdateElement(ChangeType.UpdateElement, "Warmth", Warmth);
        }

        public void DecreaseSocialNeed()
        {
            Social = Math.Max(0, Social + 0.005);
            OnUpdateElement(ChangeType.UpdateElement, "Social", Social);
        }

        public void DecreaseWarmthNeed()
        {
            Warmth = Math.Max(0, Warmth + 0.005);
            OnUpdateElement(ChangeType.UpdateElement, "Warmth", Warmth);
        }

        public override double GetNeedValue(Need need)
        {
            switch (need)
            {
                case Need.Hunger: return Hunger;
                case Need.Social: return Social;
                case Need.Lust: return Lust;
                case Need.Tiredness: return Tiredness;
                case Need.Boredom: return Boredom;
                case Need.JobFullfilment: return JobFullfilment;
                case Need.Health: return (100 - Health) / 100.0;
                default: return double.MinValue;
            }
        }

        #endregion

        public override Bitmap GetImage()
        {
            if (Sex == Sex.Female)
                return new Bitmap(Utilities.GetResourceImage("dawn.png"));
            else
                return new Bitmap(Utilities.GetResourceImage("ash.png"));
        }

        public int GetItemIndex()
        {
            return -1;
        }

        // These should not be used right now. Only the player changes the item index.
        public void IncrementItemIndex()
        {
            throw new NotImplementedException();
        }

        public void DecrementItemIndex()
        {
            throw new NotImplementedException();
        }
    }


    // Change this to a decorator for Person. Player will store a Person and references to Player will need to be changed to Player.Person.
    // This will make switching characters easier, without requiring a massive copy constructor.
    public class Player : IGetData
    {
        // Store the directions that the player has entered with the keyboard. These are set to false when the key is released.
        private bool headLeft, headRight, headUp, headDown;
        private ObjectData data;

        public event DataChangedHandler UpdateElement;

        public Person BasePerson { get; private set; }
        public bool IsAlive { get; private set; } = true;
        public OrderedPair<double> Position { get; private set; } // Store last position of the player's character. Will update as the character moves.
        public int SelectedItem { get; private set; } = 0; // Default to the first item type.

        public event EventHandler CancelData;

        public void OnCancelData()
        {
            EventHandler handler = CancelData;
            handler?.Invoke(this, new EventArgs());
        }

        public Player(Person person)
        {
            BasePerson = person;
            BasePerson.UpdateElement += BasePerson_UpdateElement;
            Position = BasePerson.Position;
            BasePerson.ClearAction();
            UpdateData();

            BasePerson.AddItem(new Item(0, 0, ItemType.Apple, 5));
        }

        // Catch BasePerson's element updates and pass these on to a DisplayPanel.
        private void BasePerson_UpdateElement(object sender, DataChangedArgs e)
        {
            OnUpdateElement(e.Type, e.ElementName, e.Value);
        }

        private void OnUpdateElement(ChangeType type, string elementName, object value)
        {
            DataChangedHandler handler = UpdateElement;
            handler?.Invoke(this, new DataChangedArgs(type, elementName, value));
        }

        public void SwapPerson(Person person)
        {
            // Swap event listening to the new BasePerson.
            BasePerson.UpdateElement -= BasePerson_UpdateElement;
            person.UpdateElement += BasePerson_UpdateElement;

            BasePerson = person;
            BasePerson.ClearAction();
            BasePerson.Stop();
            IsAlive = true;
        }

        public void Kill()
        {
            IsAlive = false;
        }

        public void SetPlayerAction(EntityAction action, bool locked = false)
        {
            if (BasePerson == null)
                return;
            BasePerson.TrySetAction(action, locked);
        }

        public void CarryOutAction(ObjectMesh objectMesh, GameTime gameTime)
        {
            if (BasePerson != null && BasePerson.Health > 0)
                BasePerson.CarryOutAction(objectMesh, gameTime); // BasePerson might be null if the player died and has not selected another person.
        }

        public void Move()
        {
            if (BasePerson == null)
                return;

            BasePerson.Move();
            Position = BasePerson.Position;
        }


        public void IncrementItemIndex()
        {
            if (BasePerson == null)
            {
                SelectedItem = -1;
            }
            else
            {
                if (BasePerson.Inventory.Count == 0)
                    SelectedItem = -1;
                else
                    SelectedItem = ++SelectedItem % BasePerson.Inventory.Count;
            }
        }

        public void DecrementItemIndex()
        {
            if (BasePerson == null)
            {
                SelectedItem = -1;
            }
            else
            {
                SelectedItem = (SelectedItem == 0) ? (BasePerson.Inventory.Count - 1) : (SelectedItem - 1);
            }
        }

        // Determines if an item exists in the player's inventory with the necessary amount.
        public bool InventoryContainsWithCount(ItemType type, int amount)
        {
            if (BasePerson == null || BasePerson.Inventory == null)
                return false;

            foreach (Item i in BasePerson.Inventory)
            {
                if (i.Type == type && i.Amount >= amount)
                    return true;
            }

            return false;
        }

        public int GetSelectedItemCount()
        {
            return BasePerson.Inventory.Count > 0 ? BasePerson.GetItemCount(BasePerson.Inventory[SelectedItem].Type) : 0;
        }

        public bool GetItemChoice(int choice, out ItemType itemType)
        {
            if (BasePerson.Inventory.Count == 0)
            {
                itemType = ItemType.Meat;
                return false;
            }
            else
            {
                itemType = BasePerson.Inventory[choice].Type;
                return true;
            }
        }

        public OrderedPair<double> GetVelocity()
        {
            if (BasePerson == null)
                return new OrderedPair<double>(0, 0);
            else
                return BasePerson.Velocity;
        }

        // Set the player's velocity and a flag for which direction was pressed.
        public void SetVelocity(Direction dir)
        {
            if (BasePerson == null)
                return;

            switch (dir)
            {
                case Direction.Up:
                    BasePerson.SetVelocity(new OrderedPair<double>(BasePerson.Velocity.X, -BasePerson.Speed));
                    headUp = true;
                    break;
                case Direction.Right:
                    BasePerson.SetVelocity(new OrderedPair<double>(BasePerson.Speed, BasePerson.Velocity.Y));
                    headRight = true;
                    break;
                case Direction.Down:
                    BasePerson.SetVelocity(new OrderedPair<double>(BasePerson.Velocity.X, BasePerson.Speed));
                    headDown = true;
                    break;
                case Direction.Left:
                    BasePerson.SetVelocity(new OrderedPair<double>(-BasePerson.Speed, BasePerson.Velocity.Y));
                    headLeft = true;
                    break;
            }
        }

        // Set the player's velocity to 0 or to the direction that is still held down.
        // Called when a key is released and sets the key direction to false.
        public void RemoveVelocity(Direction dir)
        {
            if (BasePerson == null)
                return;

            double newSpeed;
            switch (dir)
            {
                case Direction.Up:
                    newSpeed = headDown ? BasePerson.Speed : 0;
                    BasePerson.SetVelocity(new OrderedPair<double>(BasePerson.Velocity.X, newSpeed));
                    headUp = false;
                    break;
                case Direction.Right:
                    newSpeed = headLeft ? -BasePerson.Speed : 0;
                    BasePerson.SetVelocity(new OrderedPair<double>(newSpeed, BasePerson.Velocity.Y));
                    headRight = false;
                    break;
                case Direction.Down:
                    newSpeed = headUp ? -BasePerson.Speed : 0;
                    BasePerson.SetVelocity(new OrderedPair<double>(BasePerson.Velocity.X, newSpeed));
                    headDown = false;
                    break;
                case Direction.Left:
                    newSpeed = headRight ? BasePerson.Speed : 0;
                    BasePerson.SetVelocity(new OrderedPair<double>(newSpeed, BasePerson.Velocity.Y));
                    headLeft = false;
                    break;
            }
        }

        public void UpdateData()
        {
            if (BasePerson == null)
                return;

            BasePerson.UpdateData();

            //data = new ObjectData(new List<Tuple<string, object>>()
            //{
            //    new Tuple<string, object>("Health", BasePerson.Health),
            //}, 1);

            data = BasePerson.GetData();

            for (int i = 0; i < BasePerson.Inventory.Count; i++)
            {
                data.DataList.Add(new Tuple<string, object>(BasePerson.Inventory[i].Type.ToString(), 
                    BasePerson.Inventory[i].Amount));
            }

        }

        public ObjectData GetData()
        {
            if (BasePerson == null)
                return new ObjectData(new List<Tuple<string, object>>(), 0);

            return data;
        }

        public int GetItemIndex()
        {
            return SelectedItem;
        }

        public void RemoveCraftingItems(CraftingComponent[] components)
        {
            foreach (CraftingComponent c in components)
            {
                Console.WriteLine("Player's BasePerson dropping {0} {1}", c.Amount, c.Type);
                BasePerson.RemoveItem(new Item(0, 0, c.Type, c.Amount), false, false);
            }
        }

        public void AddItem(ItemType type)
        {
            if (BasePerson != null)
                BasePerson.Pickup(new Item(0, 0, type, 1));
        }
    }
}
