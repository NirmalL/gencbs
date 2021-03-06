﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using gencbs.Resources;

namespace gencbs.Jobs
{
    class Job : IComparable<Job>
    {
        private static int nextId= 0;
        public DateTime EPST{ get; set;} //Earliest Possible Start time
        public DateTime dueDate { get; set; }
        public TimeSpan duration { get; set; }
        public int delayPanaltyForHour { get; set; }
        public int jobID{ get; set;}

        public int expectedCost { get; set; }

        public Double fitness { get; set; }

        public String jobName { get; set; }
        public LinkedList<ResourceForJob> requiredResources { get; set; }

        public int cost {get; set;}


        public Job()
        {
            Interlocked.Increment(ref nextId);
            this.jobID = nextId;
            this.requiredResources = new LinkedList<ResourceForJob>();
        }

        public void addRequiredResource(ResourceType type)
        {
            ResourceForJob reqResource = new ResourceForJob(type);
            this.requiredResources.AddLast(reqResource);
        }

        /// <summary>
        /// use this method if a resource is already allocated.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="allocate"></param>
        public void addRequiredResource(ResourceType type, Resource allocate)
        {
            ResourceForJob reqResource = new ResourceForJob(type, allocate);
            this.requiredResources.AddLast(reqResource);
        }

        public Job(Job job)
        {
            this.EPST = job.EPST;
            this.dueDate = job.dueDate;
            this.duration = job.duration;
            this.jobID = job.jobID;
            this.jobName = job.jobName;
            this.delayPanaltyForHour = job.delayPanaltyForHour;
            this.requiredResources = new LinkedList<ResourceForJob>();
            this.expectedCost = job.expectedCost;

            //to avoid referencing to the same list
            //we have to create a new list every time
            foreach (ResourceForJob res in job.requiredResources)
            {
                if (res.allocated_resource != null)
                {
                    addRequiredResource(new ResourceType(res.resourceType.typeName, res.resourceType.setupCost), res.allocated_resource);
                }
                else addRequiredResource(new ResourceType(res.resourceType.typeName, res.resourceType.setupCost));
            }
        }

        //create a resource pool for the job
        public LinkedList<LinkedList<Resources.Resource>> createResourcePool()
        {
            return null;
        }

        public Job cross(Job job, int breakPoint)
        {
            return null;
        }

        //we have to consider the efficiency of the resources when gettig the intersection
        private LinkedList<TimeSlot> getIntersection()
        {
            LinkedListNode<ResourceForJob> node = this.requiredResources.First;
            LinkedList<TimeSlot> result = node.Value.allocated_resource.intersectAvailabilityList(node.Next.Value.allocated_resource.availability);
            node = node.Next;
            while (node.Next != null)
            {
                result = node.Value.allocated_resource.intersectAvailabilityList(result);
                node = node.Next;
            }
            return result;
            
        }

        /// <summary>
        /// calculate the cost added by delaying the job from the due date
        /// </summary>
        /// <returns></returns>
        private int calculateDelayPanalty()
        {
            //Console.WriteLine("Calculating delay penelty -> getting the intersection");
            LinkedList<TimeSlot> intersectionOfTimes = this.getIntersection();

            //get the least posible starting time of the job
            //Console.WriteLine("--------------------------------------------------------------------");
            //Console.WriteLine("Calculating delay penelty");
            foreach (TimeSlot slot in intersectionOfTimes)
            {
                if (slot.TimeSpan >= this.duration)
                {
                    if (slot.startTime > this.dueDate)
                    {
                        TimeSpan delay = slot.startTime - this.dueDate;
                        return delay.Hours * this.delayPanaltyForHour;
                    }
                    else return 0;
                }
            }
           // Console.WriteLine("--------------------------------------------------------------------");

            return 0;
        }

        /// <summary>
        /// get the total cost of the job
        /// </summary>
        /// <returns></returns>
        public int getCost()
        {
            //Console.WriteLine("getting cost");
            int cost = 0;
            //Console.WriteLine("--------------------------------------------------------------------");
            
            foreach (ResourceForJob res in this.requiredResources)
            {
                cost += res.allocated_resource.getCost(this.duration);
            }
            //Console.WriteLine("--------------------------------------------------------------------");
            cost += calculateDelayPanalty();
            if (cost == 0) return 1; //to avoid devision by zero
            this.cost = cost;
            //Console.WriteLine("cost of the job : " + cost);
            return cost;
        }


        public int CompareTo(Job job)
        {
            return this.cost.CompareTo(job.cost);
        }

        public override String ToString()
        {
            String id = "job ID = " + this.jobID + ", ";
            String cost = "Cost = " + this.cost + ", ";
            String res = "";
            String rName = "";
            foreach (ResourceForJob r in requiredResources)
            {
                if (r.allocated_resource != null)
                {
                    rName = r.allocated_resource.name + "";
                }
                else rName = "null";
                res = res + rName + ",";
            }
            return id + cost + res;
        }



        ////get the resource list
        //public List<String> getResourceList()
        //{
        //    List<String> list = new List<string>();
        //    foreach(res r in requredResources)
        //    {
        //        list.Add(r.resourceType);
        //    }
        //    return list;
        //}
    }
}
