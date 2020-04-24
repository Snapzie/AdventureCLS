package org.combinators.guidemo.domain.instances;

import org.combinators.guidemo.domain.Greeting;
import org.combinators.guidemo.domain.GreetingTypes;

public class GenericGreeting extends Greeting {
    public GenericGreeting() {
        super.setGreeting(GreetingTypes.generic);
    }
}