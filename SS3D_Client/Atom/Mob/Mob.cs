﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mogre;
using Lidgren.Network;
using SS3D.Atom.Mob.HelperClasses;

namespace SS3D.Atom.Mob
{
    public class Mob : Atom
    {
        
        // TODO Make these some sort of well-organized global constant
        public float walkSpeed = 1.0f;
        public float runSpeed = 2.0f;

        public Dictionary<string, HelperClasses.Appendage> appendages;
        public Appendage selectedAppendage;

        //Current animation state -- or at least the one we want to add some time to. This will need to become more robust.
        public AnimationState animState;

        public Mob()
            : base()
        {
            meshName = "male.mesh";
        }

        public virtual void initAppendages()
        {
            appendages = new Dictionary<string, Appendage>();
            appendages.Add("LeftHand", new Appendage("LHand", "LeftHand", this));
            appendages.Add("RightHand", new Appendage("RHand", "RightHand", this));
            selectedAppendage = appendages["LeftHand"];
        }

        public override void SetUp(ushort _uid, AtomManager _atomManager)
        {
            base.SetUp(_uid, _atomManager);

            animState = Entity.GetAnimationState("idle");
            animState.Loop = true;
            animState.Enabled = true;

            initAppendages();
        }

        public virtual void SetAnimationState(string state)
        {
            SetAnimationState(state, false);
        }

        public virtual void SetAnimationState(string state, bool send)
        {
            // TODO: error checking
            if (send)
                SendAnimationState(state);
            animState = Entity.GetAnimationState(state);
            animState.Loop = true;
            animState.Enabled = true;
            if (animState == null)
                animState = Entity.GetAnimationState("idle");

        }

        protected virtual void SendAnimationState(string state)
        {
            NetOutgoingMessage message = CreateAtomMessage();
            message.Write((byte)AtomMessage.Extended);
            message.Write((byte)MobMessage.AnimationState);
            message.Write(state);
            SendMessage(message);
        }

        protected virtual void HandleAnimationState(NetIncomingMessage message)
        {
            //If receiving animation state updates for our own mob, discard them.
            if(!attached)
                SetAnimationState(message.ReadString());
        }

        public override void Update()
        {
            base.Update();

            // Update Animation. Right now, anything animated will have to be updated in entirety every tick.
            TimeSpan t = atomManager.gameState.lastUpdate - atomManager.gameState.now;
            animState.AddTime((float)t.TotalMilliseconds / 1000f);

            //Update every tick
            updateRequired = true;
        }

        public override void HandleKC_W(bool state)
        {
            base.HandleKC_W(state);
            if (state==true)
                SetAnimationState("trns_walk", true);
            else
                SetAnimationState("idle", true);
        }
       
        public override void HandleKC_S(bool state)
        {
            base.HandleKC_S(state);
            if (state==true)
                SetAnimationState("trns_walk", true);
            else
                SetAnimationState("idle", true);
        }

        protected override void HandleExtendedMessage(NetIncomingMessage message)
        {
            MobMessage mobMessageType = (MobMessage)message.ReadByte();
            switch (mobMessageType)
            {
                case MobMessage.AnimationState:
                    HandleAnimationState(message);
                    break;
                case MobMessage.SelectAppendage:
                    HandleSelectAppendage(message);
                    break;
                default: break;
            }
        }

        /// <summary>
        /// Sets selected appendage to what is contained in the message
        /// </summary>
        /// <param name="message">Incoming netmessage</param>
        protected virtual void HandleSelectAppendage(NetIncomingMessage message)
        {
            SetSelectedAppendage(message.ReadString());
        }

        /// <summary>
        /// Sets selected appendage to the appendage named
        /// </summary>
        /// <param name="appendageName">Appendage name</param>
        protected virtual void SetSelectedAppendage(string appendageName)
        {
            if (appendages.Keys.Contains(appendageName))
                selectedAppendage = appendages[appendageName];
        }
    }
}
