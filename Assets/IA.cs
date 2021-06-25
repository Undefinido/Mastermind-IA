using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System.Linq;

public class IA : MonoBehaviour
{
    public InputField UserInputField;
    public Text OutputText;

    public static int codeLength = 4;

    public static float CROSSOVER_PROBABILITY = 0.5f;
    public static float CROSSOVER_THEN_MUTATION_PROBABILITY = 0.03f;
    public static float PERMUTATION_PROBABILITY = 0.03f;
    public static int MAX_POP_SIZE = 60;
    public static int MAX_GENERATIONS = 3;

    private int[,] possibleValuesPerPosition;


    public void playMastermind()
    {
        string codeToGuess = UserInputField.text;
        if (codeToGuess.Length == 4 && int.TryParse(codeToGuess, out _))
        {
            mastermindAI(codeToGuess);
        }
    }

    public void playGeneticAlgorithmMastermind()
    {
        string codeToGuess = UserInputField.text;
        if (codeToGuess.Length == 4 && int.TryParse(codeToGuess, out _))
        {
            mastermindGeneticAlgorithmAI(codeToGuess);
        }
    }

    private void mastermindAI(string TOGUESS)
    {
        int turns = 0;
        string code = Random.Range(0, 10000).ToString("0000");
        possibleValuesPerPosition = new int[codeLength,10];
        for(int i = 0; i < codeLength; i++)
        {
            for(int j = 0; j < 10; j++)
            {
                possibleValuesPerPosition[i, j] = 0;
            }
        }

        (int, int) BacOfCode = bullsAndCows(code, TOGUESS);
        string newCode;

        while (BacOfCode != (4, 0))
        {
            if(BacOfCode.Item2 > 0){
                newCode = permute(code, TOGUESS, BacOfCode);
            }
            else
            {
                newCode = mutate(code, TOGUESS, BacOfCode);
            }
;
            if (totalBACValue(BacOfCode) < totalBACValue(bullsAndCows(newCode, TOGUESS)))
            {
                code = newCode;
                BacOfCode = bullsAndCows(code, TOGUESS);
            }
            turns++;
        }
        OutputText.text = "After " + turns + " turns, AI has reached to the solution using normal algorithm";
    }

    private string permute(string code, string TOGUESS, (int,int) bullsAndCowsOfCode)
    {
        char[] codeCopy = code.ToCharArray();

        int randomPosition1 = Random.Range(0, codeLength);
        int randomPosition2 = Random.Range(0, codeLength);
        while(randomPosition1 == randomPosition2)
        {
            randomPosition2 = Random.Range(0, codeLength);
        }

        while (possibleValuesPerPosition[randomPosition2, (int)char.GetNumericValue(code[randomPosition1])] == -1)
        {
            randomPosition1 = Random.Range(0, codeLength);
            randomPosition2 = Random.Range(0, codeLength);
            while (randomPosition1 == randomPosition2)
            {
                randomPosition2 = Random.Range(0, codeLength);
            }
        }

        char auxiliar = code[randomPosition1];
        codeCopy[randomPosition1] = codeCopy[randomPosition2];
        codeCopy[randomPosition2] = auxiliar;

        string newCode = new string(codeCopy);
        (int,int) BAC = bullsAndCows(newCode, TOGUESS);

        if (BAC.Item2 >= bullsAndCowsOfCode.Item2)
        {
            possibleValuesPerPosition[randomPosition1, (int)char.GetNumericValue(codeCopy[randomPosition1])] = -1;
            possibleValuesPerPosition[randomPosition2, (int)char.GetNumericValue(codeCopy[randomPosition2])] = -1;
            //set as not correct
        }

        return newCode;
    }

    private string mutate(string code, string TOGUESS, (int,int) bullsAndCowsOfCode)
    {
        char[] aux = code.ToCharArray();
        int index = Random.Range(0, codeLength);
        int value = Random.Range(0, 10);
        
        for (int i = 0; i < 10; i++) {
            if(possibleValuesPerPosition[index,i] != -1)
            {
                value = i;
            }
        }

        /*  this breaks the code somehow
         *  int idx = 0;
         * while(possibleValuesPerPosition[index,value] == -1)
        {
            value = Random.Range(0, 10);
            /*idx++;
            if(idx == 10)
            {
                break;
            }
        }*/
        aux[index] = (char) (value + 48);   //char conversion

        string newCode = new string(aux);
        (int, int) BAC = bullsAndCows(newCode, TOGUESS);

        if(BAC.Item2 >= bullsAndCowsOfCode.Item2 && BAC.Item1 <= bullsAndCowsOfCode.Item1)
        {
            possibleValuesPerPosition[index, value] = -1;
            //set as not correct
        }

        return newCode;
    }

    public int totalBACValue((int,int) BAC)
    {
        return BAC.Item1*2 + BAC.Item2;
    }

    //--------------- GENETIC ALGORITHM APPROACH ------------------------------------------

    private void mastermindGeneticAlgorithmAI(string TOGUESS)
    {
        int turns = 0;
        string code = Random.Range(0, 10000).ToString("0000");

        (int, int) result = bullsAndCows(code, TOGUESS);
        var population = new List<string>();
        population.Add(code);

        while (result != (4, 0))
        {
            List<string> eligibles = geneticEvolution(population, MAX_POP_SIZE, codeLength, TOGUESS);

            code = eligibles[0];
            turns++;

            result = bullsAndCows(code, TOGUESS);
        }

        OutputText.text = "After " + turns + " generations, AI has reached to the solution using GE Algorithm";
    }

    private (int, int) bullsAndCows(string IAGuess, string TOGUESS)
    {
        char[] IAGCopy = IAGuess.ToCharArray();
        char[] TOGCopy = TOGUESS.ToCharArray();

        int Bulls = 0, Cows = 0;
        for (int i = 0; i < IAGuess.Length; i++)
        {
            if (IAGuess[i] == TOGUESS[i])
            {
                Bulls++;
                //mark them with different chars to not match again
                IAGCopy[i] = 'a';
                TOGCopy[i] = 'b';
            }
        }
        foreach (var code in IAGCopy)
        {
            if (TOGCopy.Contains(code))
            {
                Cows++;
                for (int i = 0; i < TOGCopy.Length; i++)
                {
                    if (code == TOGCopy[i])
                    {
                        TOGCopy[i] = 'b';
                    }
                }
            }
        }
        (int, int) result = (Bulls, Cows);
        return result;
    }

    private string crossover(string code1, string code2)
    {
        string newcode = "";
        foreach (var i in Enumerable.Range(0, codeLength))
        {
            if (Random.value > CROSSOVER_PROBABILITY)
            {
                newcode += code1[i];
            }
            else
            {
                newcode += code2[i];
            }
        }
        return newcode;
    }

    private string mutate2(string code)
    {
        char[] aux = code.ToCharArray();
        int index = Random.Range(0, codeLength);
        int value = Random.Range(0, 10);
        aux[index] = System.Convert.ToChar(value);
        return new string(aux);
    }

    private string permute2(string code)
    {
        char[] aux = code.ToCharArray();
        foreach (var i in Enumerable.Range(0, codeLength))
        {
            if (Random.value <= PERMUTATION_PROBABILITY)
            {
                var index1 = Random.Range(0, codeLength);
                var index2 = Random.Range(0, codeLength);
                var auxiliarValue = code[index1];
                aux[index1] = code[index2];
                aux[index2] = auxiliarValue;
            }
        }
        return new string(aux);
    }

    private List<string> geneticEvolution(List<string> population, int popSize, int codeLength, string TOGUESS)
    {
        population = generatePopulation(population, popSize, codeLength);
        List<string> chosenOnes = new List<string>();
        List<string> sons = new List<string>();

        //disorder list to make the solution not converge
        population = disorderList(population);

        //produce childs
        for (int i = 0; i < population.Count; i++)
        {
            if (i == population.Count - 1)
            {
                sons.Append(population[i]);
                break;
            }
            string son = crossover(population[i], population[i + 1]);
            if (Random.value <= CROSSOVER_THEN_MUTATION_PROBABILITY)
            {
                son = mutate2(son);
            }
            son = permute2(son);
            sons.Add(son);
        }

        //create list with the total BAC score and the code
        List<(int, string)> popScore = new List<(int, string)>();
        int BACresultSon;
        
        foreach (var c in sons)
        {
            BACresultSon = totalBACValue(bullsAndCows(c, TOGUESS));
            popScore.Add((BACresultSon, c));
        }

        //order by the best fit
        popScore = popScore.OrderByDescending(x => x.Item1).ToList();

        List<string> eligibles = new List<string>();

        foreach ((int, string) p in popScore)
        {
            eligibles.Add(p.Item2);
        }
        eligibles.Take((eligibles.Count / 2));  //take only the first half of the population which are better

        return eligibles;
    }

    private List<string> generatePopulation(List<string> population, int popSize, int codeLength)
    {
        for (int i = population.Count; i < popSize; i++)
        {
            string code = "";
            for (int j = 0; j < codeLength; j++)
            {
                code += Random.Range(0, 10);
            }
            population.Add(code);
        }
        return population;
    }

    private List<string> disorderList(List<string> l)
    {
        List<string> listCopy = l;
        List<string> result = new List<string>();

        while (listCopy.Count > 0)
        {
            int val = Random.Range(0, listCopy.Count);
            result.Add(listCopy[val]);
            listCopy.RemoveAt(val);
        }
        return result;
    }

}
