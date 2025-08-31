local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local switcher = {
    name="MadelineCrystal/MCrystalSwitcher",
    texture="objects/MadelineCrystal/switcher/switch01",
    placements={
        name="switcher",
        data={
            toCrystal=true,
            fromCrystal=true,
            legacyMode = false
        }
    },
    fieldInformation={
        legacyMode={default=false}
    },
    depth=2000,

    selection=function(_,entity) 
        return utils.rectangle(entity.x-8,entity.y-12,16,24)
    end
}

-- function switcher.sprite(room, entity)
--     return drawableSprite.fromTexture("objects/MadelineCrystal/switcher/switch00", {x=entity.x, y=entity.y})
-- end

return switcher