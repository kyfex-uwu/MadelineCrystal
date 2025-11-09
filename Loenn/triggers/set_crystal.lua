return {
    name="MadelineCrystal/SetCrystalTrigger",
    placements={
        name="incrystal",
        data={
            crystal=true,
            mode=0,
            oneTime=true,
        }
    },
    fieldInformation={
        mode={
            fieldType = "integer",
            options={
                enter=0,
                exit=1
            },
            editable=false
        },
        oneTime={
            default=false,
        }
    },
    fieldOrder={
        "x","y","width","height",
        "mode","crystal","oneTime"    
    }
}
