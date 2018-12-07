using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;


namespace AI_HMM
{
    public class Hmm{
        public int nodes;
        public int numberOfSequenceSteps;

        public decimal[][] emissionProbability;
        public decimal[][] transitionProbability;
        public decimal[] initialProbability;
    }

    class Program
    {

        static Hmm hmm;
        static void Main(string[] args)
        {
            hmm = new Hmm();
            Random rand = new Random();


            Console.WriteLine("Do you want to get the information of the HMM from the file(Y/N)? :");
            if (Console.ReadLine().ToUpper() == "N") //If user does NOT wants the values be grabbed from an xml file
            {
                Console.WriteLine("How many nodes do you want?");
                Int32.TryParse(Console.ReadLine(), out hmm.nodes);
                Console.WriteLine("How many sequence steps do you want");
                Int32.TryParse(Console.ReadLine(), out hmm.numberOfSequenceSteps);

                //Sets up the emmision array
                hmm.emissionProbability = new decimal[hmm.nodes][];
                for (int i = 0; i < hmm.emissionProbability.GetLength(0); ++i)
                    hmm.emissionProbability[i] = new decimal[hmm.numberOfSequenceSteps];

                //Sets up the transition array
                hmm.transitionProbability = new decimal[hmm.nodes][];
                for (int i = 0; i < hmm.transitionProbability.GetLength(0); ++i)
                    hmm.transitionProbability[i] = new decimal[hmm.nodes];

                //Sets up the initial array
                hmm.initialProbability = new decimal[hmm.nodes];
                

                Console.WriteLine("Do you want the numbers generated randomly(Y/N)?");
                bool random = true;
                if (Console.ReadLine().ToUpper() == "N")
                    random = false;

                GenerateOrAskInitialProbability(rand, random);
                GenerateOrAskTransitionProbability(rand, random);
                GenerateOrAskEmissionProbability(rand, random);


            }
            else //Gets the valus from  XML file
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Hmm));

                StreamReader reader = new StreamReader("HMM.xml");
                hmm = (Hmm)serializer.Deserialize(reader);
                reader.Close();
            }


            Console.WriteLine("Do you want to show the probability multiplication(Y/N)?");
            bool showProb = false;
            if (Console.ReadLine().ToUpper() == "Y")
                showProb = true;

            

            //this array contains the index of the next sequence that it will get the probability of
            //starts at [0,0...0,0]
            int[] sequenceArray = new int[hmm.numberOfSequenceSteps];
            for (int i = 0; i < hmm.numberOfSequenceSteps; i++)
                sequenceArray[i] = 0;

            Draw();
            //ProbabilityPath is an array of tuples where the first value is the sequence of steps and the second value is the probability of that sequence.
            //it instantiates the size to the most probability paths that it can take even the ones it cant take such as 0
            Tuple<string, decimal>[] probabilityPath = new Tuple<string, decimal>[(int)Math.Pow(hmm.nodes, hmm.numberOfSequenceSteps)];
            
            //For each probability sequence
            for (int i = 0; i < Math.Pow(hmm.nodes, hmm.numberOfSequenceSteps); i++)
            {
                
                //Starts the probability with initial probability of the start sequencearray first index.
                decimal probability = hmm.initialProbability[sequenceArray[0]];

                for (int columnEmission = 0; columnEmission < hmm.numberOfSequenceSteps; columnEmission++)
                {
                    //for each column in the emissionProbability array of the starting sequence index that we are at
                    //Multiply the probabilty with the others.
                    probability *= hmm.emissionProbability[sequenceArray[0]][columnEmission];

                    //if column is not at 0 then multiply probability with the transition probability
                    //If it was in the first index then there would not be a transition probability , just the initial probability.
                    if (columnEmission != 0)
                        probability *= hmm.transitionProbability[sequenceArray[0]][sequenceArray[columnEmission]];

                    //Checks if the column is the last index
                    if (columnEmission == hmm.numberOfSequenceSteps - 1)
                    {
                        //Once at last index then it creates a  appends the probability sequence from int to string
                        string sequenceString = "";
                        for (int s = 0; s < sequenceArray.GetLength(0); s++)
                        {
                            //Appends the sequence int into a string
                            sequenceString += (sequenceArray[s] + 1).ToString();

                            //If user wanted to see the probability then it is printed out.
                            if (showProb)
                            {
                                if (s == 0)
                                {
                                    Console.Write(hmm.initialProbability[sequenceArray[0]] + " * ");
                                    Console.Write(hmm.emissionProbability[sequenceArray[0]][s] + " * ");
                                }
                                else
                                {
                                    Console.Write(hmm.emissionProbability[sequenceArray[0]][s] + " * ");
                                    Console.Write(hmm.transitionProbability[sequenceArray[0]][sequenceArray[s]]);
                                }
                            } 
                        }

                        //adds the sequence and the probability path to the array tuple
                        probabilityPath[i] = new Tuple<string, decimal>(sequenceString, probability);
                        Console.Write("\n" + sequenceString);
                        Console.Write(": = {0}\n", probability);
                        //goes to the next sequence
                        IncrementNumberArray(hmm.nodes, sequenceArray);
                    }

                }
            }

            //Checks which path is the most probable path
            int mostProbableIndex = 0;
            for (int i = 0; i < probabilityPath.GetLength(0); i++)
            {
                if (probabilityPath[i].Item2 > probabilityPath[mostProbableIndex].Item2)
                    mostProbableIndex = i;
            }

            Console.WriteLine("\nMost probable path is");
            Console.WriteLine("{0} with probability of {1}", probabilityPath[mostProbableIndex].Item1, probabilityPath[mostProbableIndex].Item2);
            Console.ReadLine();
        }

        //Asks for the Emission Probability of each element in matrix OR Generates the numbers randomly
        private static void GenerateOrAskEmissionProbability(Random rand, bool random)
        {
            //this rangeMaxTransition is used for making sure that when values are getting generated
            //that all transitions will add up to 1
            decimal[] rangeMaxTransition = new decimal[hmm.nodes];
            for (int i = 0; i < hmm.nodes; i++)
                rangeMaxTransition[i] = 1;
            
            for (int row = 0; row < hmm.nodes; row++)
            {
                for (int column = 0; column < hmm.numberOfSequenceSteps; column++)
                {
                    //if user wants the values to be generated randomly
                    if (random)
                    {
                        //checks if  its the last index then sets the remaining value to it.
                        if (row == hmm.nodes - 1)
                            hmm.emissionProbability[row][column] = rangeMaxTransition[column];
                        //Else it generated a double, then it is multiplied by the max Transition number we have left and the
                        //number is getting set to where the decimal precision isnt very long. which is then a string then parsed into the emiisionProbability
                        else
                        {
                            decimal.TryParse(((decimal)rand.NextDouble() * rangeMaxTransition[column]).ToString("0.0"), out hmm.emissionProbability[row][column]);
                            rangeMaxTransition[column] -= hmm.emissionProbability[row][column];
                        }

                    }
                    else
                    {
                        Console.WriteLine("What is the Emission Probability column {0} row {1}' probability? (0-1)", row + 1, column + 1);
                        decimal.TryParse(Console.ReadLine(), out hmm.emissionProbability[row][column]);
                        Draw();
                    }
                }
            }
            Draw();
        }

        //Asks for the Transition Probability of each element in matrix OR Generates the numbers randomly
        private static void GenerateOrAskTransitionProbability(Random rand, bool random)
        {
            //this rangeMaxTransition is used for making sure that when values are getting generated
            //that all transitions will add up to 1
            decimal[] rangeMaxTransition = new decimal[hmm.nodes];
            for (int i = 0; i < hmm.nodes; i++)
                rangeMaxTransition[i] = 1;

            for (int row = 0; row < hmm.nodes; row++)
            {
                for (int column = 0; column < hmm.nodes; column++)
                {
                    //if user wants the values to be generated randomly
                    if (random)
                    {
                        //checks if  its the last index then sets the remaining value to it.
                        if (row == hmm.nodes - 1)
                            hmm.transitionProbability[row][column] = rangeMaxTransition[column];
                        //Else it generated a double, then it is multiplied by the max Transition number we have left and the
                        //number is getting set to where the decimal precision isnt very long. which is then a string then parsed into the emiisionProbability
                        else
                        {
                            decimal.TryParse(((decimal)rand.NextDouble() * rangeMaxTransition[column]).ToString("0.0"), out hmm.transitionProbability[row][column]);
                            rangeMaxTransition[column] -= hmm.transitionProbability[row][column];
                        }
                    }
                    else
                    {
                        Console.WriteLine("What is the Transition Probability column {0} row {1}' probability? (0-1)", row + 1, column + 1);
                        decimal.TryParse(Console.ReadLine(), out hmm.transitionProbability[row][column]);
                        Draw();
                    }
                }
            }
        }

        //Asks for the Initial Probability of each element in matrix OR Generates the numbers randomly
        private static void GenerateOrAskInitialProbability(Random rand, bool random)
        {
            Draw();
            decimal rangeMaxInitial = 1.0m;
            
            for (int i = 0; i < hmm.nodes; i++)
            {
                if (random)
                {
                    //checks if  its the last index then sets the remaining value to it.
                    if (i == hmm.nodes - 1)
                        hmm.initialProbability[i] = rangeMaxInitial;
                    //Else it generated a double, then it is multiplied by the max Transition number we have left and the
                    //number is getting set to where the decimal precision isnt very long. which is then a string then parsed into the emiisionProbability
                    else
                    {
                        decimal.TryParse(((decimal)rand.NextDouble() * rangeMaxInitial).ToString("0.0"), out hmm.initialProbability[i]);
                        hmm.initialProbability[i] *= (rangeMaxInitial); //makes the range between Max, since columns need to add up to 1.
                        rangeMaxInitial -= hmm.initialProbability[i];
                    }
                }
                else
                {
                    Console.WriteLine("What is the Initial Probability {0}' probability? (0-1)", i + 1);
                    decimal.TryParse(Console.ReadLine(), out hmm.initialProbability[i]);
                    Draw();
                }
            }
        }

        //Basically an integer with a certain base. This function increments 1 to the array and 
        //if it passed the baseNumber then it keeps on adding to another index of array
        public static int[] IncrementNumberArray(int baseNumber, int[] toAdd)
        {
            //Start loop backwards and checks if adding 1 to the index will equal to the basenumber
            for (int i = toAdd.GetLength(0) - 1; i > -1; --i)
            {
                if(toAdd[i] + 1 >= baseNumber)
                    toAdd[i] = 0;
                else
                {
                    toAdd[i] += 1;
                    return toAdd;
                }
            }
            //if it couldnt add a number, it will return the array back without any changes.
            return toAdd;
        }

        public static void Draw()
        {
            Console.Clear();
            DrawInitialProbability();
            Console.WriteLine();
            DrawTransitionProbability();
            Console.WriteLine();
            DrawEmissionProbability();
            Console.WriteLine();
        }

        public static void DrawEmissionProbability()
        {
            Console.WriteLine("Emission Probability Matrix");
            for (int column = 0; column < hmm.nodes; column++)
            {
                Console.Write("[");
                for (int row = 0; row < hmm.numberOfSequenceSteps; row++)
                {
                    Console.Write(hmm.emissionProbability[column][row]);
                    if (row != hmm.numberOfSequenceSteps - 1)
                        Console.Write(',');
                }
                Console.Write("]\n");
            }
        }

        public static void DrawTransitionProbability()
        {
            Console.WriteLine("Transition Probability Matrix");
            for (int column = 0; column < hmm.nodes; column++)
            {
                Console.Write("[");
                for (int row = 0; row < hmm.nodes; row++)
                {
                    Console.Write(hmm.transitionProbability[column][row]);
                    if (row != hmm.nodes - 1)
                        Console.Write(',');
                }
                Console.Write("]\n");
            }
        }

        public static void DrawInitialProbability()
        {
            Console.WriteLine("Inititial Probability Matrix");
            Console.Write("[");
            for (int i = 0; i < hmm.nodes; i++)
            {
                Console.Write(hmm.initialProbability[i]);
                if (i != hmm.nodes - 1)
                    Console.Write(',');
            }
            Console.Write("]\n");
        }
    }
}

//Saving object as an XML
/*XmlSerializer xsSubmit = new XmlSerializer(typeof(Hmm));
            var xml = "";

            using (var sww = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sww))
                {
                    xsSubmit.Serialize(writer, hmm);
                    xml = sww.ToString(); // Your XML
                }
            }

            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(xml);
            xdoc.Save("HMM.xml");*/
