﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gencbs.Resources;
using gencbs.Jobs;

namespace gencbs.Scheduler.Genetic
{
    class GeneticSchedular
    {
        static int populationSize = 20;
        static int crossoverLimit = 16; // number of individuals to crossover, other set will be mutate
        public Job[] population = new Job[populationSize];
        public Job[] nextGeneration = new Job[populationSize];
        private Random randomNumber = new Random();


        //public LinkedList<ResourceType> resourcePool = PreSchedule.createResourcePool();
        public LinkedList<ResourceType> resourcePool = Test.createTestResourcePool();

        public GeneticSchedular()
        {
        }

        /// <summary>
        /// create the initial population by randomly assigning resources to jobs
        /// </summary>
        /// <param name="job"></param>
        public void createInitialGeneration(Job job)
        {
            Console.WriteLine("creating initial generation");
            for (int i = 0; i < populationSize; i++)
            {
                //Console.WriteLine("creating initial generation, individual number "+i);
                //population[i] = new Job(job);
                this.population[i] = assignRandomResources(job);
                this.population[i].cost = this.population[i].getCost();
               // Console.WriteLine("-----------------------------------------------)))))))))))");
            }
        }

        /// <summary>
        /// get a random resource of the given type from the resource pool
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Resource getRandomResource(String type)
        {
            foreach (ResourceType node in resourcePool)
            {
                if (node.typeName == type)
                {
                    return node.getRandomResource();
                }
            }
            Console.WriteLine("type could not found in the resource pool");
            return null;
        }

        /// <summary>
        /// randomly assign resources to a job
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public Job assignRandomResources(Job j)
        {
            Job job = new Job(j);
            //Console.WriteLine("assigning random resource------------------------------------" + j);
            foreach(ResourceForJob res in job.requiredResources) 
            {
                //Console.WriteLine("assign to type - " + res.resourceType.typeName);
                res.allocated_resource = getRandomResource(res.resourceType.typeName);
            }
            //Console.WriteLine(job);
            return job;
        }

        private void crossoverPopulation()
        {
            int[] selection; 
            Job[] newJobs = new Job[2];
            //Console.WriteLine("start crossovering the population----");
            for (int i = 0; i < 2; i++)
            {
                this.nextGeneration[i] = new Job(this.population[i]);
            }
            for (int i = 1; i < crossoverLimit / 2; i++)
            {
                selection = selectParentIndex();
                //Console.WriteLine("crossover parents: p1 = "+ selection[0] + " and p2 = "+ selection[1]);
                newJobs = crossover(population[selection[0]], population[selection[1]]);
                this.nextGeneration[2*i] = new Job(newJobs[0]);
                this.nextGeneration[2 * i + 1] = new Job(newJobs[1]);
            }

        }

       
        /// <summary>
        /// cross over two individuals to generate two offsprings
        /// </summary>
        public Job[] crossover(Job job1, Job job2)
        {
            Job[] offsprings = new Job[2];

            int breakPoint = randomNumber.Next(job1.requiredResources.Count);

            offsprings[0] = new Job(job1);
            offsprings[1] = new Job(job2);

            for (int i = 0; i < job1.requiredResources.Count; i++)
            {
                if (i < breakPoint)
                {
                    offsprings[0].requiredResources.ElementAt(i).allocated_resource = job2.requiredResources.ElementAt(i).allocated_resource;
                }
                else
                {
                    offsprings[1].requiredResources.ElementAt(i).allocated_resource = job2.requiredResources.ElementAt(i).allocated_resource;
                }
            }

            return offsprings;
        }

        //this won't work since we are passing the references
        public void mutation() 
        {
            for (int i = crossoverLimit; i < populationSize; i++)
            {
                this.nextGeneration[i] = mutate(this.population[i]);
            }
        }

        /// <summary>
        /// Select one randome resources from each individuals to be mutated and change them.
        /// </summary>
        public Job mutate(Job j)
        {
            Job job = new Job(j);
            int noOfResources = job.requiredResources.Count;
            int mutationResourceNo = randomNumber.Next(noOfResources);

            ResourceType mutationResourceType = job.requiredResources.ElementAt(mutationResourceNo).resourceType;


            //Console.WriteLine("mutating the " + mutationResourceType.typeName);

            Resource newSwapResource =  getRandomResource(mutationResourceType.typeName);
            job.requiredResources.ElementAt(mutationResourceNo).allocated_resource = newSwapResource;
            return job;
        } 


        /// <summary>
        /// select two parents from the initial population for crossovers, 
        /// fitness of the individuals are considered for selection
        /// </summary>
        /// <returns></returns>
        private int[] selectParentIndex()
        {

            //here only random numbers are selected,but probabilistic aproach has to be taken
            int[] selectedIndex = new int[2];
            selectedIndex[0] = randomNumber.Next(crossoverLimit - 1);
            selectedIndex[1] = randomNumber.Next(crossoverLimit - 1);
            while (selectedIndex[0] == selectedIndex[1]) //to avoid getting the same individual as parents
            {
                selectedIndex[1] = randomNumber.Next(crossoverLimit - 1);
            }

            return selectedIndex;
        }

        /// <summary>
        /// calculate the fitness function of each job and sort the population accordingly
        /// </summary>
        private void sortPopulationByFitness()
        {
            //is this fitness function good enough?
            //how can we check for the termination by this??

            /*
            Double totalFitness = 0;
            for (int i = 0; i < populationSize; i++)
            {
                population[i].cost = population[i].getCost();
                population[i].fitness = 1/population[i].cost;
                totalFitness += population[i].fitness;
            }

            for (int i = 0; i < populationSize; i++)
            {
               // population[i].fitness = 1 - ( population[i].cost / totalCost);
                population[i].fitness = population[i].fitness / totalFitness;
            }
             * 
             * */
            for (int i = 0; i < populationSize; i++)
            {
                // population[i].fitness = 1 - ( population[i].cost / totalCost);
                
                this.population[i].getCost();
            }
            Array.Sort(population);

        }

        public void assignNewPopulation()
        {
            //Console.WriteLine("replacing the older population by next generation.");
            for (int i = 0; i < populationSize; i++)
            {
                //Console.Write(i + ", ");
                this.population[i] = new Job(this.nextGeneration[i]);
            }
        }

        public void runSchedular(Job job)
        {
            createInitialGeneration(job);

            for (int i = 0; i < 10; i++)
            {

                //Console.WriteLine("population " + i + "==========================");
                //for (int j = 0; j < populationSize; j++ )
                //{
                //    Console.WriteLine(population[j]);
                //}
                sortPopulationByFitness();
                //------------------------------------
                if (population[0].cost <= job.expectedCost) break; //break since we got a good enough solution, or expected solution

                Console.WriteLine("population " + i + " -> after sorting =================================");
                for (int j = 0; j < populationSize; j++)
                {
                    Console.WriteLine(population[j]);
                }

                crossoverPopulation();
                //domutations here
                mutation();
                assignNewPopulation();
                

            }
        }
    }
}
