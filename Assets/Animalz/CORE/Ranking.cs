using System;

namespace Assets.Animalz
{
    public class Ranking
    {
        protected AiRig AiR;

        public int Rank;

        public Ranking(AiRig Air)
        {
            AiR = Air;
        }

        public void Update()
        {
            LoadVars();
        }

        protected virtual void LoadVars()
        {
            Rank = AiR.RankingSystem.Rank;            
        }

        public virtual bool GreaterThan(Ranking other)
        {
            return Rank > other.Rank;
        }

    }
}