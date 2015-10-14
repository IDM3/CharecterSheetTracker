$(function () {
    $("#AddItem").find('input[type="button"]').click(function () { AddItem(); });
});

function AddItem() {
    var idStart = "Character\.Items[";
    var itemIdEnd = "]\.ItemHeld\.Id";
    var countEnd = "]\.Count";
    var equippedEnd = "]\.Equipped";
    var addItemRow = $("#AddItem");
    var itemNumber = new Number(addItemRow.attr("data-count"));
    var itemSelectDropDown = addItemRow.find("select");
    var countItem = addItemRow.find('input[type="number"]');
    var equippedItem = addItemRow.find('input[type="checkbox"]');
    var itemSelected = itemSelectDropDown.find(":selected");
    var selectedItemId = itemSelected.val();
    var selectedItemName = itemSelected.text();
    var count = countItem.val();
    var equipped = equippedItem.attr('checked');
    if (count > 0) {
        itemSelected.remove();
        var newItemNumber = itemNumber + 1;
        addItemRow.attr("data-count", newItemNumber);

        itemSelectDropDown.attr("id", idStart + newItemNumber + itemIdEnd);
        itemSelectDropDown.attr("name", idStart + newItemNumber + itemIdEnd);

        countItem.attr("id", idStart + newItemNumber + countEnd);
        countItem.attr("name", idStart + newItemNumber + countEnd);
        countItem.val(0);
        equippedItem.attr("id", idStart + newItemNumber + equippedEnd);
        equippedItem.attr("name", idStart + newItemNumber + equippedEnd);
        equippedItem.removeAttr("checked");
        addItemRow.before(MakeItem(itemNumber, selectedItemId, selectedItemName, count, equipped));
    }
}

function MakeItem(itemInventoryId, itemId, itemName, count, equipped)
{
    var html = '<tr><td><input id="Character.Items[' + itemInventoryId + 
        '].ItemHeld.Id" name="Character.Items[' + itemInventoryId + 
        '].ItemHeld.Id" value="' + itemId + 
        '" type="hidden" /><label for="Character.Items[' + itemInventoryId + 
        '].Value">' + itemName + '</label></td><td><input id="Character.Items[' + itemInventoryId + 
        '].Count" name="Character.Items[' + itemInventoryId + '].Count" type="number" value="' + count +
        '" /></td><td><input id="Character.Items[' + itemInventoryId 
        + '].Equipped" name="Character.Items[' + itemInventoryId + '].Equipped" type="checkbox"';
    if (equipped)
    {
        html += 'checked="checked"';
    }
    html += 'value="true" /></td></tr>';
    return $(html);
}