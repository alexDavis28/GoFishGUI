using System;

namespace GoFishGUI
{
    class PlayerHand : GoFishHand
    {
        public PlayerHand()
        {
            HandType = "Player";
        }

        // Have to include for abstract class, probably a better way of doing this but short on time
        // It's never called so it's fine (hopefully)
        public override int RequestCard()
        {
            throw new NotImplementedException();
        }
    }
}
