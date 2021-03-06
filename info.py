import math
# Price, Inc, Gain
Oaks = [100, 1.1, 2]
Bushes = [20, 1.1, 0.6]
Ants = [220, 1.1, 3.6]
Birches = [440, 3, 7]
Yews = [4000, 1.1, 23]
Flowers = [2000, 1.1, 1]
Spiders = [1000, 1.1, 85]

def getNum(num):
    l = math.floor(math.log(num, 10))
    lead = num/pow(10, l)
    return "%sE%s" %(lead, l)

def price(growth, amount):
    res = 0
    for i in range(0, amount):
        res += growth[0]*pow(growth[1], i)
    print(getNum(res));

def info(growth, amount, multi):
    res = 0
    for i in range(0, amount):
        res += growth[0]*pow(growth[1], i)
    amount *= growth[2]*multi
    print("%s / %s"%(getNum(amount), getNum(res)));

def xpGain(totalBefore, add):
    res = math.log(totalBefore+add+1, 1000000) - math.log(totalBefore+1, 10000000)
    print("%s" %(res))
    return pow(10, res)*100-100

def xpTotal(total):
    res = math.log(total+1, 1000000)
    return pow(10, res)*100
def total(xp):
    res = math.log(xp/100, 10)  
    res = pow(1000000, res)
    return res-1
