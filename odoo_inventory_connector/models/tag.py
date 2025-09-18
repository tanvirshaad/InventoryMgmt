from odoo import models, fields, api

class Tag(models.Model):
    _name = 'inventory.connector.tag'
    _description = 'Inventory Item Tag'
    
    name = fields.Char(string='Name', required=True)
    inventory_id = fields.Many2one('inventory.connector.inventory', string='Inventory', required=True, ondelete='cascade')
    
    # Color for display in kanban views
    color = fields.Integer(string='Color Index')
    
    # Items with this tag
    item_ids = fields.Many2many('inventory.connector.item', string='Items')
    item_count = fields.Integer(compute='_compute_item_count', string='Item Count')
    
    _sql_constraints = [
        ('inventory_name_unique', 'UNIQUE(inventory_id, name)', 'Tag names must be unique per inventory')
    ]
    
    @api.depends('item_ids')
    def _compute_item_count(self):
        for tag in self:
            tag.item_count = len(tag.item_ids)