using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassTest : MonoBehaviour
{
    // Start is called before the first frame update

    public enum AnimalType
    {
        None,

        Cat,

        Dog,


    }

    public class Animal
    {
        private string _Name;
        private string _Food;
        private AnimalType _AnimalType;


        public AnimalType mAnimalType
        {
            get
            {
                return _AnimalType;
            }
            set
            {
                _AnimalType = value;
                if (_AnimalType == AnimalType.Cat)
                {
                    _Name = "cat";

                }
                else if (_AnimalType == AnimalType.Dog)
                {
                    _Name = "dog";
                }
            }
        }



        public void EatFood()
        {
            Debug.Log(this._Name + " is eating " + this._Food);
        }

        public Animal(string food)
        {
            this._Food = food;
        }

        public void PlaySound()
        {
            if (_AnimalType == AnimalType.Cat)
            {
                Debug.Log(this._Name + " is meow ");

            }
            else if (_AnimalType == AnimalType.Dog)
            {
                Debug.Log(this._Name + " is wolf ");
            }
        }
        void start()
        {
            Animal cat = new Animal("fishbone");
            cat.mAnimalType = AnimalType.Cat;
            cat.EatFood();
            cat.PlaySound();
        }
    }
}
