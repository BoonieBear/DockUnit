﻿using System;
using BoonieBear.DeckUnit.CommLib;

namespace BoonieBear.DeckUnit.Core
{
    public class DeckUnitDataObserver:CommLib.IObserver<CustomEventArgs>
    {
        public void Handle(object sender, CustomEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}