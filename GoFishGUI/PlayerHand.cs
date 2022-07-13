using System;

namespace GoFishGUI
{
    class PlayerHand : GoFishHand
    {
        public PlayerHand()
        {
            HandType = "Player";
        }

        public override int RequestCard()
        {
            throw new NotImplementedException();
        }
    }
}
