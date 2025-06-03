using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoundsWithBots.CardPickerAIs.Weighted {
    public interface IWeightedCardProcessor {
        float GetWeight(CardInfo card, Player player);
    }
}
