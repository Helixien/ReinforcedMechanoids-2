﻿using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ReinforcedMechanoids
{
    public abstract class RaidStrategyWorker_ImmediateAttackWithCertainPawnKind : RaidStrategyWorker_ImmediateAttack
	{
		public override bool CanUseWith(IncidentParms parms, PawnGroupKindDef groupKind)
		{
			if (!PawnGenOptionsWithRequiredPawns(parms.faction, groupKind).Any())
			{
				return false;
			}
			if (!base.CanUseWith(parms, groupKind))
			{
				return false;
			}
			return true;
		}
		protected abstract bool MatchesRequiredPawnKind(PawnKindDef kind);
		protected virtual int MinRequiredPawnsForPoints(float pointsTotal, Faction faction = null)
        {
			return 1;
        }

        protected virtual int MaxRequiredPawnsForPoints(float pointsTotal, Faction faction = null)
        {
            return 99999;
        }
        public override float MinimumPoints(Faction faction, PawnGroupKindDef groupKind)
		{
			return Mathf.Max(base.MinimumPoints(faction, groupKind), CheapestRequiredPawnCost(faction, groupKind));
		}

		public override float MinMaxAllowedPawnGenOptionCost(Faction faction, PawnGroupKindDef groupKind)
		{
			return CheapestRequiredPawnCost(faction, groupKind);
		}

		private float CheapestRequiredPawnCost(Faction faction, PawnGroupKindDef groupKind)
		{
			IEnumerable<PawnGroupMaker> enumerable = PawnGenOptionsWithRequiredPawns(faction, groupKind);
			if (!enumerable.Any())
			{
				Log.Error("Tried to get MinimumPoints for " + GetType().ToString() + " for faction " + faction.ToString() + " but the faction has no groups with the required pawn kind. groupKind=" + groupKind);
				return 99999f;
			}
			float num = 9999999f;
			foreach (PawnGroupMaker item in enumerable)
			{
				foreach (PawnGenOption item2 in item.options.Where((PawnGenOption op) => MatchesRequiredPawnKind(op.kind)))
				{
					if (item2.Cost < num)
					{
						num = item2.Cost;
					}
				}
			}
			return num;
		}

		public override bool CanUsePawnGenOption(float pointsTotal, PawnGenOption g, List<PawnGenOptionWithXenotype> chosenGroups, Faction faction = null)
		{
			if (chosenGroups != null)
			{
                if (chosenGroups.Count < MinRequiredPawnsForPoints(pointsTotal, faction)
					&& !MatchesRequiredPawnKind(g.kind))
                {
                    return false;
                }
                if (MatchesRequiredPawnKind(g.kind) && MaxRequiredPawnsForPoints(pointsTotal, faction) <= chosenGroups
                    .Where(x => x.option.kind == g.kind).Count())
                {
                    return false;
                }
            }
            return true;
        }


		private IEnumerable<PawnGroupMaker> PawnGenOptionsWithRequiredPawns(Faction faction, PawnGroupKindDef groupKind)
		{
			if (faction.def.pawnGroupMakers == null)
			{
				return Enumerable.Empty<PawnGroupMaker>();
			}
			return faction.def.pawnGroupMakers.Where((PawnGroupMaker gm) => gm.kindDef == groupKind && gm.options != null && gm.options.Any((PawnGenOption op) => MatchesRequiredPawnKind(op.kind)));
		}
	}
}
