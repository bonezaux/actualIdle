import random
import threading
import time
import math

euler = 2.7182818284590455
statList = ["Health", "Attack", "HealthRegen"]


#Modifies a value by all given modifiers
def modify(modifiers, modName, value):
    val2 = value
    for mod in map(lambda mod: mod.getModA(modName), modifiers):
        val2 += mod
        
    product = 1
    for mod in map(lambda mod: mod.getModF(modName), modifiers):
        product *= mod
        
    val2 *= product
    return val2

#Returns an array, [mod added, mod multiply]
def getModification(modifiers, modName):
    sums = 0
    for mod in map(lambda mod: mod.getModA(modName), modifiers):
        sums += mod
        
    product = 1
    for mod in map(lambda mod: mod.getModF(modName), modifiers):
        product *= mod
        
    return [sums, product]

class Modifier:
    'Modifiers for anything'
    def __init__(self, name, modifiersF, modifiersA={}):
        self.name = name
        self.modifiersF = modifiersF
        self.modifiersA = modifiersA
    
    def getModF(self, modName):
        if(modName in self.modifiersF):
            return self.modifiersF[modName]
        else:
            return 1
            
    def getModA(self, modName):
        if(modName in self.modifiersA):
            return self.modifiersA[modName]
        else:
            return 0

def safe_cast(val, to_type, default=None):
    try:
        return to_type(val)
    except (ValueError, TypeError):
        return default

class Formula(): #Everything in formulas is given in forest values
    def __init__(self):
        pass
    
    def calculate(self, number, forest): 
        return 0;

class FormulaLinear(Formula):
    def __init__(self, base, proportionality):
        self.proportionality = proportionality
        self.base = base

    def calculate(self, number, forest):
        return forest.getValue(self.base) + forest.getValue(self.proportionality)*number

class FormulaLogistic(Formula):
    def __init__(self, limit, speed, start):
        self.limit = limit;
        self.speed = speed;
        self.start = start;

    def calculate(self, number, forest):
        x = number
        lf = forest.getValue(self.limit)
        spd = forest.getValue(self.speed)
        start = forest.getValue(self.start)
        etox = start*pow(euler, lf*spd*x)
        return (lf*etox)/(etox+lf-start)

class FormulaLimitedExp(Formula):
    def __init__(self, limit, growth):
        self.limit = limit
        self.growth = growth

    def calculate(self, number, forest):
        x = number
        lf = forest.getValue(self.limit)
        a = (forest.getValue(self.growth)*0.693147)
        
        res = lf-pow(euler, -a*x)*lf
        return res

        
class Resources():
    def __init__(self, table):
        self.table = table

    def canAfford(self, forest, amount):
        for thing, thingAmount in self.table.items():
            if(thingAmount < 0 and forest.things[thing] < -thingAmount*amount):
                return False
        return True

    def apply(self, forest, amount):
        for thing in self.table:
            forest.things[thing] += self.table[thing]*amount

    def print(self, forest, amount):
        for thing, thingAmount in self.table.items():
            print("\t%s %s" %(thingAmount*amount, thing))

    def text(self, forest, amount):
        res = ""
        for thing, thingAmount in self.table.items():
            res += "\t"+str(thingAmount*amount) + " " + str(thing)
        return res

class ResourcesIncrement(Resources):
    def __init__(self, table, inc, base):
        self.table = table #Table is a dict of things and amount of those corresponding things
        self.inc = inc
        self.base = base

    #Returns what the price is to be modified by for a single purchase of the thing.
    #The modification is increment to the power of the base
    #Lift is how much more buys are to be calculated than the current buys when this is calculated
    def mod(self, lift, forest):
        return pow(self.inc, forest.getValue(self.base)+lift)

    #Returns the price. thingAmount is the base price.
    def incPrice(self, thing, thingAmount, amount, forest):
        res = 0
        for x in range(0,amount):
            res += thingAmount*self.mod(x, forest)
        return res
       
    def canAfford(self, forest, amount):
        for thing, thingAmount in self.table.items():

            if(thingAmount < 0 and forest.things[thing] < -self.incPrice(thing, thingAmount, amount, forest)):
                return False
        return True

    def apply(self, forest, amount):
        for thing, thingAmount in self.table.items():
            forest.things[thing] += self.incPrice(thing, thingAmount, amount, forest)
        
    
    def print(self, forest, amount):
        for thing, thingAmount in self.table.items():
            print("\t%s %s" %(round(self.incPrice(thing, thingAmount, amount, forest), 2), thing))

    def text(self, forest, amount):
        res = ""
        for thing, thingAmount in self.table.items():
            res += "\t"+str(round(self.incPrice(thing, thingAmount, amount, forest), 2)) + " " + str(thing)
        return res

#Returns a Resources object based on a scalar and a table of resources and formulas. Example table:
#   [
#       {"seeds" : Formula()}
#   ]
class ResourcesScale(object):
    def __init__(self, table):
        self.table = table

    def getResources(self, scale, forest):
        resTable = {}
        for thing, formula in self.table.items():
            resTable[thing] = formula.calculate(scale, forest)
        return Resources(resTable)

#FORESTOBJECTS
class ForestObject(object):
    def __init__(self, forest, addedThings, addedFormulas, name, price):
        self.name = name;
        forest.things[name] = 0
        if(type(addedThings) == list):
            self.addedThings = addedThings
            self.addedFormulas = addedFormulas
        else:
            self.addedThings = [addedThings]
            self.addedFormulas = [addedFormulas]
        self.forest = forest
        self.price = price
        #Whether this ForestObject is unlocked. This should be updated in loop if it isn't just a flip thing.
        self.unlocked = False
        
        for stat in statList:
            if(not (name+stat in forest.values)):
                forest.values[name+stat] = 0
        
    def loop(self):
        for x in range(0,len(self.addedThings)):
            if(self.addedThings[x] != None):
                self.forest.things[self.addedThings[x]] += self.addedFormulas[x].calculate(self.forest.things[self.name], self.forest)

    def buy(self, amount):
        if(self.price.canAfford(self.forest, amount)):
            print("You bought %s %s for" %(amount, self.name) )
            self.price.print(self.forest, amount)
            self.price.apply(self.forest, amount)
            forest.things[self.name] += amount
            forest.values["boughtThings"] += amount
            return True
        else:
            print("You don't have enough to buy %s %s! You need" %(amount, self.name) );
            self.price.print(self.forest, amount);
            return False

    def echo(self):
        res = self.name + ": " + str(round(self.forest.things[self.name],2))
        for x in range(len(self.addedThings)):
            res += ", +" + str(round(self.addedFormulas[x].calculate(self.forest.things[self.name], self.forest), 2)) + " " + str(self.addedThings[x]) + "/t"
        print(res)


    def echoPrice(self):
        res = self.name + ": "
        if self.price != None:
            res += self.price.text(self.forest, 1)
            print(res)
        else:
            res += "None"

    def getStats(self):
        stats = {}
        for stat in statList:
            stats[stat] = self.forest.getValue(self.name + stat);
        return stats;

class DruidObject(ForestObject):
    def __init__(self, forest, addedThings, addedFormulas, name, price, xp):
        super(DruidObject, self).__init__(forest, addedThings, addedFormulas, name, price)
        self.xp = xp

    def buy(self, amount):
        if(super(DruidObject, self).buy(amount)):
            forest.addXp("Druidcraft", amount*self.xp*(1+forest.getValue("wandlevel")*0.01))

class Item():
    itemList = []
    
    def __init__(self, name, changedValues, text):
        self.name = name
        self.changedValues = changedValues
        self.text = text

    def loop(self, forest):
        pass

    def get(self, forest):
        for entry in self.changedValues:
            forest.changeValue(entry["value"],entry["change"])

    def lose(self, forest):
        for entry in self.changedValues:
            forest.changeValue(entry["value"],-entry["change"])

    def echo(self, forest):
        print(self.text)

#DOABLES
class Doable(object):
    def __init__(self, name, resourceChange, reqs, forest, text, noText, unlockedThings, remainUnlocked):
        self.name = name
        self.resourceChange = resourceChange
        self.forest = forest
        self.text = text
        self.reqs = reqs
        self.noText = noText
        self.unlocked = False;
        self.unlockedThings = unlockedThings
        self.remainUnlocked = remainUnlocked
        self.addedDoables = []
        self.addedUnlocks = []

    def addDoables(self, doables):
        self.addedDoables += doables
        return self

    def addUnlocks(self, unlocks):
        self.addedUnlocks += unlocks
        return self
    
    def testReqs(self):
        
        doReqs = True
        for req in self.reqs:
            if(not self.forest.testReq(req)):
                doReqs = False
        return doReqs

    def do(self):
        if((self.resourceChange == None or self.resourceChange.canAfford(self.forest, 1)) and self.testReqs()):
            if(self.resourceChange != None):
                self.resourceChange.print(self.forest, 1)
                self.resourceChange.apply(self.forest, 1)
            print(self.text)
            self.unlocked=self.remainUnlocked;
            for unlockedThing in self.unlockedThings:
                self.forest.objects[unlockedThing].unlocked = True
            for doable in self.addedDoables:
                self.forest.unlockDoable(doable);
            for unlock in self.addedUnlocks:
                self.forest.unlockUnlockable(unlock);
            return True
        else:
            print(self.noText)
            return False

class DoableChangeThings(Doable):
    def __init__(self, name, resourceChange, reqs, forest, text, noText, unlockedThings, remainUnlocked, changeTable):
        super(DoableChangeThings, self).__init__(name, resourceChange, reqs, forest, text, noText, unlockedThings, remainUnlocked)
        self.changeTable = changeTable
        print(name)

    def do(self):
        if(super(DoableChangeThings, self).do()):
            for entry in self.changeTable:
                forest.things[entry["target"]] += forest.things[entry["source"]]*entry["value"]
                forest.things[entry["source"]] -= forest.things[entry["source"]]

class DoableFlowerHeal(Doable):
    def __init__(self, name, resourceChange, reqs, forest, text, noText, unlockedThings, remainUnlocked, healPerNectar):
        super(DoableFlowerHeal, self).__init__(name, resourceChange, reqs, forest, text, noText, unlockedThings, remainUnlocked)
        self.healPerNectar = healPerNectar

    def do(self):
        if(super(DoableFlowerHeal, self).do()):
            amount = input("How much nectar do you want to use? Each heals %s" %(self.healPerNectar))
            amount = safe_cast(amount, float, 0)
            if(forest.things["Nectar"] >= amount):
                prevHp = forest.druid.hp
                forest.druid.addHp(amount*self.healPerNectar, None)
                postHp = forest.druid.hp
                forest.things["Nectar"] -= amount
                print("You healed %s for %s Nectar, hp: %s -> %s" %(amount*self.healPerNectar, amount, prevHp, postHp))
            
#UNLOCKABLE           
class Unlockable():
    def __init__(self, name, resourceReq, forest, addedDoables, addedUnlocks, text, unlockedThings):
        self.name = name;
        self.resourceReq = resourceReq;
        self.forest = forest;
        self.addedDoables = addedDoables;
        self.addedUnlocks = addedUnlocks;
        self.unlocked = False
        self.text = text;
        self.unlockedThings = unlockedThings

    def loop(self):
        if(self.resourceReq.canAfford(self.forest, 1)):
            for doable in self.addedDoables:
                self.forest.unlockDoable(doable);
            for unlock in self.addedUnlocks:
                self.forest.unlockUnlockable(unlock);
            self.unlocked = False;
            for unlockedThing in self.unlockedThings:
                self.forest.objects[unlockedThing].unlocked = True
            print(self.text);

class Fighter(object):
    def __init__(self, maxhp, attack, defense, name, reward, xp):
        self.hp = maxhp
        self.maxhp = maxhp
        self.attack = attack
        self.defense = defense
        self.name = name
        self.reward = reward
        self.xp = xp

    def takeDamage(self, damage, fight):
        resDamage = damage-self.defense
        if resDamage > 0:
            self.hp -= resDamage
            return True
        return False

    def addHp(self, hp, fight):
        if(hp < 0 or self.hp == self.maxhp):
            return False
        self.hp += hp
        if(self.hp > self.maxhp):
            self.hp = self.maxhp
        return True



class Fight(object):
    def __init__(self, druid, fighter2):
        self.druid = druid
        self.fighter2 = fighter2

        if(self.druid.hp <= 0):
            print(self.fighter2.name + " won the fight!")
        else:
            print("You won the fight! You got:")
            self.fighter2.reward.print(self.druid.forest, 1)
            self.fighter2.reward.apply(self.druid.forest, 1)
            for skill, xp in self.fighter2.xp.items():
                self.druid.forest.addXp(skill, xp)
                print(skill + ": +" + str(xp) + "xp. ->" + str(self.druid.forest.xp[skill]))
                

class FighterTemplate(object):
    def __init__(self, name, minLevel, maxLevel, minHp, maxHp, minAttack, maxAttack, minDefense, maxDefense, rewardScale, xpScale):
        self.name = name
        self.minLevel = minLevel
        self.levelDiff = float(maxLevel-minLevel)
        self.minHp = minHp
        self.hpDiff = maxHp-minHp
        self.minAttack = minAttack
        self.attackDiff = maxAttack-minAttack
        self.minDefense = minDefense
        self.defenseDiff = maxDefense-minDefense
        self.rewardScale = rewardScale
        self.xpScale = xpScale

    def exists(self, level):
        return minLevel<=level<=maxLevel
    
    def create(self, level, forest):
        scale = (level-self.minLevel)/self.levelDiff
        xp = {}
        for skill, formula in self.xpScale.items():
            xp[skill] = formula.calculate(scale, forest)
        reward = self.rewardScale.getResources(scale, forest)
        return Fighter(self.minHp + self.hpDiff*scale, self.minAttack + self.attackDiff*scale, self.minDefense + self.defenseDiff*scale, self.name, reward, xp)

class Location(object):
    wolf1 = FighterTemplate("wolf", 4, 3, 1001, 1000, 6, 5, 2, 1, ResourcesScale({"Seeds" : FormulaLinear("!I50", "!F0")}), {"Druidcraft" : FormulaLinear("!I2", "!I0")})
    
    locations = []
    
    def __init__(self, name, x, y, templates):
        self.name = name
        self.x = x
        self.y = y
        self.templates = templates
        Location.locations += [self]

    def getLocation(x, y):
        dists = []
        lowestDist = 1000000;
        num = -1
        i = 0
        for loc in Location.locations:
            if(abs(x-loc.x)+abs(y-loc.y) < lowestDist):
                lowestNum = i
            i += 1
        return Location.locations[lowestNum]


#FOREST
class Forest(Fighter):
    skills = ["Druidcraft"]
    
    def __init__(self):
        self.things = {}
        self.objects = {}
        self.doables = {}
        self.unlocks = {}
        self.values = {}
        self.groups = {}
        self.xp = {}
        for skill in Forest.skills:
            self.xp[skill] = 1
        self.items = []
        self.x = 984.0;
        self.y = 873.0;
        self.boss = Location.wolf1.create(1, self);
        self.fighting = False
        self.modifiers = []
        self.hp = 0.0
        self.maxhp = 0.0

    # Returns the stats of the Druid of this Forest
    def getStats(self):
        stats = {}

        for statName in statList:
            stats[statName] = 0
        
        for name, thing in self.things.items():
            thingStats = self.objects[name].getStats()
            for statName in statList:
                stats[statName] += thingStats[statName]*thing

        for statName in statList:
            stats[statName] = modify(self.modifiers, statName, stats[statName])

        return stats
        
    
    def unlockDoable(self, doable):
        if(doable in self.doables):
            self.doables[doable].unlocked = True;
        else:
            print("Doable %s doesnt exist" %(doable))
            
    def unlockUnlockable(self, unlock):
        if(unlock in self.unlocks):
            self.unlocks[unlock].unlocked = True;
        else:
            print("Unlockable %s doesnt exist" %(unlock))

    def loop(self):
        for name,unlock in self.unlocks.items():
            if(unlock.unlocked):
                unlock.loop()
        for name,obj in self.objects.items():
            obj.loop();
        for item in self.items:
            item.loop(self)
        self.maxhp = self.getStats()["Health"]
        self.hp += self.getStats()["HealthRegen"]/20
        if(self.hp > self.maxhp):
            self.hp = self.maxhp
        

    def addItem(self, item):
        self.items += [item]
        item.get(self)

    def addThing(self, thing):
        self.objects[thing.name] = thing;
        
    def addDoable(self, doable):
        self.doables[doable.name] = doable;

    def addUnlockable(self, unlockable):
        self.unlocks[unlockable.name] = unlockable;

    def addGroup(self, groupName):
        parts = list(self.objects.keys()) + list(self.doables.keys())
        self.groups[groupName] = parts

    def addXp(self, skillName, xp):
        if skillName in Forest.skills:
            self.xp[skillName] += xp

    def buyThing(self, thing, amount):
        self.objects[thing].buy(amount);


    def echoBoss(self):
        print("Boss:")
        print(self.boss.name)
        print(self.boss.hp)
        
    def listThings(self):
        i = 0
        print("")
        for name,amount in self.things.items():
            if self.objects[name].unlocked:
                self.objects[name].echo()
                i += 1
                if(i == 4):
                    i = 0
                    print("")

    def listPrices(self):
        for name,amount in self.things.items():
            if(self.objects[name].unlocked):
                self.objects[name].echoPrice()

    def listItems(self):
        for item in self.items:
            item.echo(forest)
    
    def listDoables(self):
        print("You can:")
        for name,doable in self.doables.items():
            if(doable.unlocked):
                print("\t%s" %(name));

    def listSkills(self):
        for skill in Forest.skills:
            print("%s\tlvl %s\t%sxp" %(skill, self.getValue("lvl"+skill), round(self.xp[skill], 2))) 
    
    def do(self, doable):
        if(self.doables[doable] and self.doables[doable].unlocked):
            self.doables[doable].do()
    
    def getAmount(self, thing):
        self.objects[thing].echo()

    def getValue(self, value):
        if(value.startswith("lvl")):
            if value[3:] in Forest.skills:
                return int(math.log10(self.xp[value[3:]]))
        elif(value.startswith("thing")):
            if value[5:] in list(self.things.keys()):
                return self.things[value[5:]]
        elif(value.startswith("item")):
            if(value[4:] in list(self.items.keys())):
                return 1
        elif(value.startswith("!I")):
            return int(value[2:])
        elif(value.startswith("!F")):
            return float(value[2:])
        elif(value.startswith("!B")):
            return bool(value[2:])
        return modify(self.modifiers, value, self.values[value]);

    def changeValue(self, value, change):
        if value in list(self.values.keys()):
            self.values[value] += change
        else:
            self.values[value] = change

    def testReq(self, req):
        value1 = self.getValue(req.split("_")[0])
        compare = req.split("_")[1]
        value2 = self.getValue(req.split("_")[2])

        if(compare == ">="):
            return value1>=value2
        elif(compare == "<="):
            return value1 <= value2
        elif(compare == ">"):
            return value1 > value2
        elif(compare == "<"):
            return value1 < value2
        elif(compare == "="):
            return value1 == value2
        else:
            return False

    def createEnemy(self):
        loc = Location.getLocation(self.x, self.y)
        template = loc.templates[random.randint(0,len(loc.templates)-1)]
        print(template)
        return template.create(1,self)
        
#START OF RUNTIME BELOW
running = True

{
    "things" : [
        {
            "name" : "Dead Leaves",
            "yield" : {None : None},
            "resources" : None,
            "description" : "Some dead leaves"
        },
        {
            "name" : "Berries",
            "yield" : {None : None},
            "resources" : None,
            "description" : "Some berries"
        }
    ]
}

forest = Forest()



forest.modifiers += [Modifier("Debug", {"BushesGain":10000, "OaksGain":10000, "BirchesGain":10000})]
## FOREST THINGS
forest.addThing(ForestObject(forest, None, Formula(), "Organic Material", None))
forest.values["boughtThings"] = 2


#Level 1
forest.values["BushesGain"] = 0.6
forest.values["BushesAttack"] = 1
forest.values["BushesHealth"] = 0.2
forest.addThing(DruidObject(forest, "Organic Material", FormulaLinear("!I0", "BushesGain"), "Bushes", ResourcesIncrement({"Organic Material" : -10}, 1.1, "boughtThings"), 1))

#Level 2
forest.values["OaksGain"] = 2
forest.values["OaksHealth"] = 1
forest.addThing(DruidObject(forest, ["Organic Material"], [FormulaLinear("!I0", "OaksGain")], "Oaks", ResourcesIncrement({"Organic Material" : -50}, 1.1, "boughtThings"), 3))
forest.addDoable(Doable("Create Oak", Resources({"Organic Material" : -300, "Oaks" : 1}), [], forest, "You eat some berries. Suddenly, an oak sapling shoots from your bum and lands in the dirt, growing pleasantly.", "You need 300 seeds to do this.",["Oaks"], False).addDoables(["Create Birch", "bleg"]))
forest.doables["Create Oak"].unlocked = True;

#Level 3
forest.values["BirchesGain"] = 7
forest.values["BirchesAttack"] = 2
forest.addDoable(Doable("Create Birch", Resources({"Organic Material" : -900, "Birches" : 1}), [], forest, "You fuse fifteen seeds into a very white seed, but first, it fails a bunch of times. You end up with a birch", "You need 600 seeds to do this.",["Birches"], False).addDoables(["Create Yew"]))
forest.addThing(DruidObject(forest, "Organic Material", FormulaLinear("!I0", "BirchesGain"), "Birches", ResourcesIncrement({"Organic Material" : -220}, 1.1, "boughtThings"), 27))

#Level 4
forest.values["YewsGain"] = 23
forest.values["YewsHealth"] = 4
forest.values["YewsAttack"] = 2
forest.addDoable(Doable("Create Yew", Resources({"Organic Material" : -2700, "Yews" : 1}), [], forest, "You touch the ground, and a yew springs from it.", "You need 1500 seeds to do this.",["Yews"], False).addDoables(["Create Flower"]))
forest.addThing(DruidObject(forest, "Organic Material", FormulaLinear("!I0", "YewsGain"), "Yews", ResourcesIncrement({"Organic Material" : -950}, 1.1, "boughtThings"), 100))

#Level 5
forest.values["FlowersGain"] = 0.4
forest.values["FlowersHealth"] = 1
forest.values["FlowersHealthRegen"] = 0.5
forest.addThing(ForestObject(forest, None, Formula(), "Nectar", None));
forest.addDoable(Doable("Create Flower", Resources({"Organic Material" : -3000, "Flowers" : 1}), [], forest, "You whooshbadoosh on a bunch of seeds with your wand, and they turn into a flower.", "You need 4000 seeds to do this.",["Flowers", "Nectar"], False).addDoables(["Create Spider", "Flower Heal"]))
forest.addThing(DruidObject(forest, "Nectar", FormulaLinear("!I0", "FlowersGain"), "Flowers", ResourcesIncrement({"Organic Material" : -2000}, 1.1, "boughtThings"), 9))

#Level 7
forest.values["SpidersGain"] = 85
forest.values["SpidersAttack"] = 9
forest.addDoable(Doable("Create Spider", Resources({"Organic Material": -8100, "Spiders" : 1}), [], forest, "Eeeeek! Even druids are afraid of spiders!", "You need 3 Flies to do this.",["Spiders"], False))
forest.addThing(DruidObject(forest, "Organic Material", FormulaLinear("!I0", "SpidersGain"), "Spiders", ResourcesIncrement({"Organic Material" : -4100}, 1.1, "boughtThings"), 150))



forest.addDoable(DoableFlowerHeal("Flower Heal", Resources({}), [], forest, "", "", [], True, 0.01))
forest.objects["Bushes"].unlocked = True;
forest.objects["Organic Material"].unlocked = True
forest.things["Bushes"] = 2

forest.addGroup("forest")
Item.itemList += [Item("wand", [{"value":"wandlevel","change":1}], "Wand\t+1 wand level")]      ##ITEM #0

forest.addItem(Item.itemList[0])
    
class CalcThread(threading.Thread):
    def __init__(self, threadID, name, counter, forest):
        threading.Thread.__init__(self)
        self.threadID = threadID
        self.name = name
        self.counter = counter
        self.forest = forest

    def run(self):
        while(running):
            
            time.sleep(0.2)
            
            self.counter += 1
            self.forest.loop()
        print("terminating")
    
#End of cairn running thread

thread1 = CalcThread(1, "calc", 1, forest)

thread1.start()

while True:
    l = input("\nWhat do you do? ")
    if l.startswith("do"):
        if len(l.split(" ")) == 1:
            forest.listDoables()
        elif l[3:].strip() in forest.doables.keys():
            forest.do(l[3:].strip())
        else:
            print("No doable by name" + l[3:].strip());
    elif l.startswith("things"):
        forest.listThings()
    elif l.startswith("create"):
        if len(l.split(" ")) == 1:
            forest.listPrices()
        elif l[7:].strip() in forest.things.keys():
            thing = l[7:].strip()
            if forest.objects[thing].price == None:
                print("This cannot be created.")
            else:
                amount = input("How many? ")
                forest.buyThing(thing,int(amount))
    elif l.startswith("items"):
        forest.listItems()
    elif l.startswith("skills"):
        forest.listSkills()
    elif l.startswith("hp"):
        print(str(forest.hp) + " / " + str(forest.maxhp))
    elif l.startswith("fight"):
        forest.echoBoss() 
    elif l.startswith("boss"):
        forest.echoBoss()
    elif l.startswith("stats"):
        for statName, stat in forest.getStats().items():
            print("%s: %s" %(statName, stat))
