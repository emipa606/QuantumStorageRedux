# [Quantum Storage Redux (Continued)](https://steamcommunity.com/sharedfiles/filedetails/?id=2777221052)

![Image](https://i.imgur.com/buuPQel.png)
Update of cheetahs mod
https://steamcommunity.com/sharedfiles/filedetails/?id=1965568159

![Image](https://i.imgur.com/pufA0kM.png)
	
![Image](https://i.imgur.com/Z4GOv8H.png)
# 1.1 rebuild live, thanks to @Patient Someone


Adds upgradeable storage and logistics solutions, based around the concept of Quantum Storage.

This is RT Quantum Storage reworked from scratch. This mod adds the following devices:


- Quantum Stockpiles
Devices that are placed underneath stockpile zones in order to store more than one item or stack of items in a given space. Each stockpile connected to it's own local quantum network. When powered, these stockpiles scan zone spaces adjacent to them and, if the zone's filter allows it, pull any items onto themselves. Items are steadily superposed, so each one remains fully accessible, and nothing catastrophic will happen even if the power supply fails.

- Quantum Warehouses
Devices that are placed underneath stockpile zones to form global quantum network of the all quantum warehouses, on top of acting like quantum stockpiles themselves. When powered, a zone's warehouse makes sure that all warehouses in the global quantum network are evenly filled and tries to minimize the amount of incomplete item stacks.

- Quantum Relays
Stand-alone devices that can connect to the global quantum network to transfer items back and forth. When powered, relays scan adjacent stockpile zone (and storage building) spaces and send items in them to the connected network, provided that network has cells that accepts them and has higher or equal priority (compared to the scanned space). At the same time, connected network will try to push items onto the relay, as defined by relay's own filter, only one stacks of each allowed item, if relay's priority is higher or equal to that of the network's zone.



Differences from the original Quantum Storage mod:


-  No separate chunk storage and general storage. Any storage can store any item, chunk, resource, weapon or even corpse.
-  Instead of connecting all stockpiles underneath stockpile zone with warehouse, all warehouses connected to the global quantum network, no matter where they are on map. Global quantum network works only with warehouses and relays.
-  Relays pulls items from the network only by one stack of each allowed item. If item on the relay becomes disallowed, it pushes to the network automatically.



![Image](https://i.imgur.com/PwoNOj4.png)


-  See if the the error persists if you just have this mod and its requirements active.
-  If not, try adding your other mods until it happens again.
-  Post your error-log using [https://steamcommunity.com/workshop/filedetails/?id=818773962]HugsLib[/url] or the standalone [url=https://steamcommunity.com/sharedfiles/filedetails/?id=2873415404](Uploader) and command Ctrl+F12
-  For best support, please use the Discord-channel for error-reporting.
-  Do not report errors by making a discussion-thread, I get no notification of that.
-  If you have the solution for a problem, please post it to the GitHub repository.
-  Use [https://github.com/RimSort/RimSort/releases/latest](RimSort) to sort your mods



[https://steamcommunity.com/sharedfiles/filedetails/changelog/2777221052]![Image]((https://img.shields.io/github/v/release/emipa606/QuantumStorageRedux?label=latest%20version&style=plastic&color=9f1111&labelColor=black))
