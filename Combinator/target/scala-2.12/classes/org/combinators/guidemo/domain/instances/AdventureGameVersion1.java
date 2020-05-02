package org.combinators.guidemo.domain.instances;

import org.combinators.guidemo.domain.AdventureGame;
import org.combinators.guidemo.domain.AbilityTypes;

public class AdventureGameVersion1 extends AdventureGame {
    public AdventureGameVersion1() {
        super.setAbility(AbilityTypes.none);
    }
}