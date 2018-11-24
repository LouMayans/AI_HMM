using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_HMM
{
    class Program
    {
        static int nodes;
        static int numberOfSequenceSteps;

        static float[][] emissionProbability;
        static float[][] transitionProbability;
        static float[] initialProbability;

        static void Main(string[] args)
        {
            Console.WriteLine("How many nodes do you want?");
            Int32.TryParse( Console.ReadLine(), out nodes);
            Console.WriteLine("How many sequence steps do you want");
            Int32.TryParse(Console.ReadLine(), out numberOfSequenceSteps);

            //Sets up the array
            emissionProbability = new float[nodes][];
            for (int i = 0; i < emissionProbability.GetLength(0); ++i)
                emissionProbability[i] = new float[numberOfSequenceSteps];

            //Sets up the array
            transitionProbability = new float[nodes][];
            for (int i = 0; i < transitionProbability.GetLength(0); ++i)
                transitionProbability[i] = new float[nodes];

            //Sets up the array
            initialProbability = new float[nodes];
            Random rand = new Random();

            Console.WriteLine("Do you want the numbers generated randomly(Y/N)?");
            bool random = true;
            if (Console.ReadLine().ToUpper() == "N")
                random = false;

            Console.WriteLine("Do you want to show the probability multiplication(Y/N)?");
            bool showProb = false;
            if (Console.ReadLine().ToUpper() == "Y")
                showProb = true;

            Draw();
            float rangeMaxInitial = 1f;
            //Asks for the Initial Probability of each element in matrix OR Generates the numbers randomly
            for (int i = 0; i < nodes; i++)
            {
                if (random)
                {
                    if (i == nodes - 1)
                        initialProbability[i] = rangeMaxInitial;
                    else
                    {
                        float.TryParse((rand.NextDouble() * rangeMaxInitial).ToString("0.0"), out initialProbability[i]);
                        initialProbability[i] *= (rangeMaxInitial); //makes the range between Max, since columns need to add up to 1.
                        rangeMaxInitial -= initialProbability[i];
                    }
                }
                else
                {
                    Console.WriteLine("What is the Initial Probability {0}' probability? (0-1)", i + 1);
                    float.TryParse(Console.ReadLine(), out initialProbability[i]);
                    Draw();
                }
            }
            //Asks for the Transition Probability of each element in matrix OR Generates the numbers randomly
            float[] rangeMaxTransition = new float[nodes];
            for (int i = 0; i < nodes; i++)
                rangeMaxTransition[i] = 1f;

            for (int row = 0; row < nodes; row++)
            {
                for (int rowIndex = 0; rowIndex < nodes; rowIndex++)
                {
                    if (random)
                    {
                        if (row == nodes - 1)
                            transitionProbability[row][rowIndex] = rangeMaxTransition[rowIndex];
                        else
                        {
                            float.TryParse((rand.NextDouble() * rangeMaxTransition[rowIndex]).ToString("0.0"), out transitionProbability[row][rowIndex]);
                            rangeMaxTransition[rowIndex] -= transitionProbability[row][rowIndex];
                        }
                    }
                    else
                    {
                        Console.WriteLine("What is the Transition Probability column {0} row {1}' probability? (0-1)", row + 1, rowIndex + 1);
                        float.TryParse(Console.ReadLine(), out transitionProbability[row][rowIndex]);
                        Draw();
                    }
                }
            }

            for (int i = 0; i < nodes; i++)
                rangeMaxTransition[i] = 1f;
            //Asks for the Emission Probability of each element in matrix OR Generates the numbers randomly
            for (int row = 0; row < nodes; row++)
            {
                for (int rowIndex = 0; rowIndex < numberOfSequenceSteps; rowIndex++)
                {
                    if (random)
                    {
                        if (row == nodes - 1)
                            emissionProbability[row][rowIndex] = rangeMaxTransition[rowIndex];
                        else
                        {
                            float.TryParse((rand.NextDouble() * rangeMaxTransition[rowIndex]).ToString("0.0"), out emissionProbability[row][rowIndex]);
                            rangeMaxTransition[rowIndex] -= emissionProbability[row][rowIndex];
                        }
                        
                    }
                    else
                    {
                        Console.WriteLine("What is the Emission Probability column {0} row {1}' probability? (0-1)", row + 1, rowIndex + 1);
                        float.TryParse(Console.ReadLine(), out emissionProbability[row][rowIndex]);
                        Draw();
                    }
                }
            }
            Draw();

            //this array contains the index of the next sequence that it will get the probability of
            int[] sequenceArray = new int[numberOfSequenceSteps];
            for (int i = 0; i < numberOfSequenceSteps; i++)
                sequenceArray[i] = 0;

            Tuple<string, float>[] probabilityPath = new Tuple<string, float>[(int)Math.Pow(nodes, numberOfSequenceSteps)];
            //For each probability sequence loop
            for (int i = 0; i < Math.Pow( nodes, numberOfSequenceSteps); i++)
            {
                //sequence += i.ToString();
                float probability = initialProbability[sequenceArray[0]];

                for (int columnEmission = 0; columnEmission < numberOfSequenceSteps; columnEmission++)
                {

                    probability *= emissionProbability[sequenceArray[0]][columnEmission];

                    if (columnEmission != 0)
                        probability *= transitionProbability[sequenceArray[0]][sequenceArray[columnEmission]];
                        
                    if(columnEmission == numberOfSequenceSteps - 1)
                    {
                        string sequenceString = "";
                        for (int s = 0; s < sequenceArray.GetLength(0); s++)
                        {
                            if (showProb)
                            {
                                if (s == 0)
                                {
                                    Console.Write(initialProbability[sequenceArray[0]] + " * ");
                                    Console.Write(emissionProbability[sequenceArray[0]][s] + " * ");
                                }
                                else
                                {
                                    Console.Write(emissionProbability[sequenceArray[0]][s] + " * ");
                                    Console.Write(transitionProbability[sequenceArray[0]][sequenceArray[s]] + "\n");
                                }
                            }

                            sequenceString += (sequenceArray[s]+1).ToString();
                            if (s != sequenceArray.GetLength(0) - 1)
                                sequenceString += ":";
                        }
                        probabilityPath[i] = new Tuple<string, float>(sequenceString, probability);
                        Console.Write(sequenceString);
                        Console.Write(" = {0}\n", probability);
                        IncrementNumberArray(nodes, sequenceArray);
                    }
                    
                }
            }

            
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

        public static int[] IncrementNumberArray(int baseNumber, int[] toAdd)
        {
            for (int i = toAdd.GetLength(0) - 1; i > -1; --i)
            {
                if(toAdd[i] + 1 >= baseNumber)
                {
                    toAdd[i] = 0;
                }
                else
                {
                    toAdd[i] += 1;
                    return toAdd;
                }
            }
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
            for (int column = 0; column < nodes; column++)
            {
                Console.Write("[");
                for (int row = 0; row < numberOfSequenceSteps; row++)
                {
                    Console.Write(emissionProbability[column][row]);
                    if (row != numberOfSequenceSteps - 1)
                        Console.Write(',');
                }
                Console.Write("]\n");
            }
        }

        public static void DrawTransitionProbability()
        {
            Console.WriteLine("Transition Probability Matrix");
            for (int column = 0; column < nodes; column++)
            {
                Console.Write("[");
                for (int row = 0; row < nodes; row++)
                {
                    Console.Write(transitionProbability[column][row]);
                    if (row != nodes - 1)
                        Console.Write(',');
                }
                Console.Write("]\n");
            }
        }

        public static void DrawInitialProbability()
        {
            Console.WriteLine("Inititial Probability Matrix");
            Console.Write("[");
            for (int i = 0; i < nodes; i++)
            {
                Console.Write(initialProbability[i]);
                if (i != nodes - 1)
                    Console.Write(',');
            }
            Console.Write("]\n");
        }
    }
}
