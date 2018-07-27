using ActualIdle.Entity_Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualIdle {
    /// <summary>
    /// An upgrade.
    /// When it is bought, the value Upgrade[Name]Bought is set to 1
    /// Has three injects, unlocked, bought and ownedLoop.
    ///  - unlocked returns a bool and all must be true for it to be unlocked.
    ///  - bought is run when the Upgrade is bought.
    ///  - ownedLoop is run every loop when the Upgrade is owned.
    /// </summary>
    public class Upgrade : Entity {
        private string _postDescriptionText;

        /// <summary>
        /// Text shown before you buy the upgrade.
        /// </summary>
        public string PreDescriptionText { get; set; }
        /// <summary>
        /// Description of an upgrade after you buy it.
        /// </summary>
        public string PostDescriptionText { get => _postDescriptionText ?? PreDescriptionText; set => _postDescriptionText = value; }
        
        public bool Owned { get => Amount>0; set => throw new NotImplementedException(); }

        /// <summary>
        /// CodeInjects can be added afterwards.
        /// </summary>
        /// <param name="forest"></param>
        /// <param name="name">Name of upgrade</param>
        /// <param name="preDescriptionText">Text shown before you buy the upgrade.</param>
        /// <param name="postDescriptionText">Text shown after you buy the upgrade. If set to null, will be equal to preDescriptionText.</param>
        /// <param name="price">Price of the upgrade</param>
        /// <param name="modifiers">Modifiers added when the upgrade is bought.</param>
        /// <param name="requirements">Requirements for having the Upgrade unlocked. For when Lambda is overdoing it..</param>
        public Upgrade(Forest forest, string name, string preDescriptionText, string postDescriptionText, Resources price, Modifier modifier = null, string requirements = null)
            : base(forest, name, E.GRP_UPGRADES, 0){
            this.Forest = forest;
            Name = name;
            PreDescriptionText = preDescriptionText;
            PostDescriptionText = postDescriptionText;
            if (postDescriptionText == null)
                PostDescriptionText = preDescriptionText;
            Add(new EExtBuyable(price, false));
            if(modifier != null)
                Add(new EExtModifier(modifier));
            Add(new EExtRequirements(requirements));
        }

        public override void Loop() {
            base.Loop();
        }

        public override void Echo(bool writeDescription=true) {
            Console.WriteLine(" --- " + Name + " --- [" + (Owned ? "Owned" : "Available") + "]");
            if (Owned)
                Console.WriteLine(PostDescriptionText);
            else {
                Console.WriteLine(PreDescriptionText);
                Extensions[E.EEXT_BUYABLE].Echo();
            }
            
        }
    }
}
