from odoo import models, fields, api

class Item(models.Model):
    _name = 'inventory.connector.item'
    _description = 'Inventory Item'
    _order = 'import_date desc, name'
    
    name = fields.Char(string='Name', required=True)
    inventory_id = fields.Many2one('inventory.connector.inventory', string='Inventory', required=True, ondelete='cascade')
    external_id = fields.Char(string='External ID', required=True)
    import_date = fields.Datetime(string='Import Date')
    last_update = fields.Datetime(string='Last Update')
    active = fields.Boolean(default=True)
    
    # Relationships
    field_value_ids = fields.One2many('inventory.connector.field.value', 'item_id', string='Field Values')
    tag_ids = fields.Many2many('inventory.connector.tag', string='Tags')
    
    # Computed fields for easier access to common field values
    text_fields = fields.Text(compute='_compute_text_fields', string='Text Fields', store=False)
    numeric_fields = fields.Text(compute='_compute_numeric_fields', string='Numeric Fields', store=False)
    boolean_fields = fields.Text(compute='_compute_boolean_fields', string='Boolean Fields', store=False)
    
    _sql_constraints = [
        ('inventory_external_id_unique', 'UNIQUE(inventory_id, external_id)', 'External ID must be unique per inventory')
    ]
    
    def name_get(self):
        result = []
        for record in self:
            result.append((record.id, f"{record.inventory_id.name}: {record.name}"))
        return result
    
    @api.depends('field_value_ids')
    def _compute_text_fields(self):
        for item in self:
            text_values = item.field_value_ids.filtered(lambda v: v.field_type in ['text', 'multiline'])
            if text_values:
                item.text_fields = ', '.join([f"{v.field_name}: {v.text_value or ''}" for v in text_values])
            else:
                item.text_fields = ''
    
    @api.depends('field_value_ids')
    def _compute_numeric_fields(self):
        for item in self:
            numeric_values = item.field_value_ids.filtered(lambda v: v.field_type == 'numeric')
            if numeric_values:
                item.numeric_fields = ', '.join([f"{v.field_name}: {v.numeric_value}" for v in numeric_values])
            else:
                item.numeric_fields = ''
    
    @api.depends('field_value_ids')
    def _compute_boolean_fields(self):
        for item in self:
            boolean_values = item.field_value_ids.filtered(lambda v: v.field_type == 'boolean')
            if boolean_values:
                item.boolean_fields = ', '.join([f"{v.field_name}: {'Yes' if v.boolean_value else 'No'}" for v in boolean_values])
            else:
                item.boolean_fields = ''