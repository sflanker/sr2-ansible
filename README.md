> "I can't explain philotic physics to you. Half of it nobody understands anyway. What matters is we built the ansible. The official name is Philotic Parallax Instantaneous Communicator, but somebody dredged the name ansible out of an old book somewhere and it caught on. Not that most people even know the machine exists."
> 
> "That means that ships could talk to each other even when they're across the solar system." said Ender.
> 
> "It means," said Graff, "that ships could talk to each other even when they're across the galaxy."

# SR2 Ansible

SR2 Ansible is a mod for SimpleRockets 2 that makes it possible for Flight Programs to  broadcast and receive messages to and from other crafts. Unfortunately, it doesn't currently live up to its name in that it's only relevant when the crafts are within 10 kilometers of eachother (closer if they are within the atmosphere).

![](https://raw.githubusercontent.com/sflanker/sflanker.github.io/master/images/entanglement-propagation.jpg)

## Installation

Download the `Ansible.sr2-mod` file from the latest release and move it to the SimpleRockets 2 Mods folder (`C:\Users\<YOUR_USER_NAME>\AppData\LocalLow\Jundroo\Mods\` on Windows, or `/Users/<YOUR_USER_NAME>/Library/Application Support/com.jundroo.SimpleRockets2/Mods/` on Mac OS). Start SimpleRockets 2 and enable the Mod using the Mods main menu option.

## Usage

Once Installed the Ansible part will be available at the top of the Gizmos part list (sorry it currently just looks like a block). To use the Ansible you will need to edit the Flight Program on the Ansible part. This Flight Program can act like a relay between the Flight Program on your Command Pod and other crafts.

### Transmitting a Message

To broadcast a message to other crafts it must be broadcast from the Flight Program on the Ansible part, **and the message string must start with "tx_".** Restricting inter-craft broadcasts to those messages with this prefix prevents excessive cross talk between Flight Programs with existing messages. *Broadcasting messages uses power, so don't forget to include a battery in your craft, and broadcast messages wisely.*

You can tweak the amount of power used for each message with the following XML:

```xml
<!-- 1000 is the default power consumption -->
<Ansible.PhiloticParallax powerConsumptionPerMessage="1000" />
```

### Receiving a Message

To receive an inter-craft message, and a `receive` instruction with the message string `rx_<your-message-name>`. The Ansible automatically matches any message sent as `tx_foo` with receivers for `rx_foo` (this is necessary to prevent feedback loops in the message transmission mechanism). You can then execute any code you like when that `receiver` is triggered. *Receiving messages does not require any power.*